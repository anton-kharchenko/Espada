using System.Reflection;
using Espada.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Transient);

        return services;
    }
}