using Espada.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Espada.Domain.Events;
using Espada.Domain.SeedWork;
using Espada.Application.UseCases.Imports.EventHandlers;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.UseCases.Imports;
using Espada.Application.Contracts.Billing;
using Espada.Application.Services.Billing;

namespace Espada.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApplicationLayer(this IServiceCollection services)
    {
        Assembly assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Transient);
        services.AddAutoMapper(_ => { }, assembly);
        services.AddScoped<IDomainEventHandler<ImportJobRequestedDomainEvent>, ImportJobRequestedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ImportStageScheduledDomainEvent>, ImportStageScheduledDomainEventHandler>();
        services.AddScoped<IImportPipelineStageExecutor, ImportPipelineStageExecutor>();
        services.AddScoped<IImportAdmissionPolicy, AllowImportAdmissionPolicy>();
        services.AddScoped<IUsageMeter, NoOpUsageMeter>();

        return services;
    }
}