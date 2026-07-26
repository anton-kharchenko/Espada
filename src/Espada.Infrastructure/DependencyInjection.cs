using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Infrastructure.Database;
using Espada.Db.Constants;
using Espada.Infrastructure.Extensions;
using Espada.Infrastructure.Repositories;
using Espada.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Espada.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
            services.AddDbContext<EspadaDbContext>((serviceProvider, options) =>
                options.UseNpgsql(
                    serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly("Espada.Db");
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
                    }));
            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<EspadaDbContext>());
            services.AddScoped<IEmbeddingVectorStore, EmbeddingVectorStore>();
            services.AddRepositories();
        }
    }
}