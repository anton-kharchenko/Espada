using Espada.Application.Behaviors;
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

        return services;
    }
}