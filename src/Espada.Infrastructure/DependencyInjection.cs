using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddSingleton<IClock, SystemClock>();
            services.AddDbContext<EspadaDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(EspadaDbContext).Assembly.FullName)));
            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<EspadaDbContext>());

            return services;
        }
    }
}