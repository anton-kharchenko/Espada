using Espada.Application.Contracts.Billing;
using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Services;
using Espada.Billing.Webhooks;
using Espada.Billing.Webhooks.Handlers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stripe;
using System.Reflection;

namespace Espada.Billing;

public static class BillingServiceCollectionExtensions
{
    public static void AddEspadaBilling(this IServiceCollection services, IConfiguration configuration)
    {
        Assembly assembly = typeof(BillingServiceCollectionExtensions).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Transient);

        IConfigurationSection section = configuration.GetSection(BillingConstants.SectionName);
        BillingOptions configured = section.Get<BillingOptions>() ?? new BillingOptions();
        services
            .AddOptions<BillingOptions>()
            .Bind(section)
            .Validate(options => options.IsValid(), "Billing configuration is incomplete.")
            .ValidateOnStart();

        if (!configured.Enabled)
        {
            return;
        }

        if (!string.Equals(StripeConfiguration.ApiVersion, BillingConstants.RequiredStripeApiVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Stripe.net API version must be {BillingConstants.RequiredStripeApiVersion}, but is {StripeConfiguration.ApiVersion}.");
        }

        services.AddHttpClient("Stripe");
        services.AddSingleton(serviceProvider =>
        {
            BillingOptions options = serviceProvider.GetRequiredService<IOptions<BillingOptions>>().Value;
            HttpClient httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("Stripe");
            return new StripeClient(new StripeClientOptions
            {
                ApiKey = options.StripeSecretKey,
                HttpClient = new SystemNetHttpClient(httpClient)
            });
        });

        services.AddScoped<IStripeBillingProvider, StripeBillingProvider>();
        services.AddScoped<IStripeWebhookIngestor, StripeWebhookIngestor>();
        services.AddScoped<IStripeWebhookProcessor, StripeWebhookProcessor>();
        services.AddScoped<IStripeWebhookHandler, CheckoutCompletedWebhookHandler>();
        services.AddScoped<IStripeWebhookHandler, SubscriptionWebhookHandler>();
        services.AddScoped<IStripeWebhookHandler, InvoicePaymentWebhookHandler>();
        services.AddScoped<IImportAdmissionPolicy, BillingImportAdmissionPolicy>();
        services.AddScoped<IUsageReconciliationProcessor, StripeUsageReconciliationProcessorService>();
    }
}