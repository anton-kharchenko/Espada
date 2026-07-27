using Espada.Application.Behaviors;
using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Ingestion;
using Espada.Application.Services;
using Espada.Application.Services.Billing;
using Espada.Application.UseCases.Imports;
using Espada.Application.UseCases.Imports.EventHandlers;
using Espada.Domain.Events;
using Espada.Domain.SeedWork;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
        services.AddScoped<IImportPipelineStageExecutorService, ImportPipelineStageExecutorService>();
        services.AddScoped<IImportAdmissionPolicy, AllowImportAdmissionPolicy>();
        services.AddScoped<IUsageMeterService, NoOpUsageMeterService>();

        return services;
    }
}