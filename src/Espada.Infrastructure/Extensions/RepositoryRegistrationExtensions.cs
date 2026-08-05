using Espada.Application.Contracts.Persistence;
using Espada.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure.Extensions
{
    internal static class RepositoryRegistrationExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            Type contractMarker = typeof(IWorkspaceRepository);
            Type implementationMarker = typeof(WorkspaceRepository);

            Type[] implementationTypes = implementationMarker.Assembly
                .GetTypes()
                .Where(type =>
                    type is { IsClass: true, IsAbstract: false } && type.Namespace == implementationMarker.Namespace)
                .ToArray();

            foreach (Type implementationType in implementationTypes)
            {
                Type[] contractTypes = implementationType
                    .GetInterfaces()
                    .Where(type =>
                        type.Namespace == contractMarker.Namespace &&
                        type.Name.EndsWith("Repository", StringComparison.Ordinal))
                    .ToArray();

                foreach (Type contractType in contractTypes)
                {
                    services.AddScoped(contractType, implementationType);
                }
            }
        }
    }
}