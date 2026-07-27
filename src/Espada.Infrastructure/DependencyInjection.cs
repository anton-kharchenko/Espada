using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Db.Constants;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Extensions;
using Espada.Infrastructure.Options;
using Espada.Infrastructure.Repositories;
using Espada.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Espada.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, string connectionString, IConfiguration? configuration = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddSingleton<IClockService, SystemClockService>();
            services.AddSingleton(_ =>
            {
                NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
                dataSourceBuilder.UseVector();
                return dataSourceBuilder.Build();
            });
            services.AddDbContext<EspadaDbContext>((serviceProvider, options) =>
                options.UseNpgsql(
                    serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly("Espada.Db");
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
                        npgsqlOptions.UseVector();
                    }));
            services.AddDbContext<WorkspaceContextSearchDbContext>((serviceProvider, options) =>
                options.UseNpgsql(
                    serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                    npgsqlOptions => npgsqlOptions.UseVector())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<EspadaDbContext>());
            services.AddScoped<IEmbeddingVectorStore, EmbeddingVectorStore>();
            services.AddScoped<IWorkspaceContextSearchStore, WorkspaceContextSearchStore>();

            OptionsBuilder<EmbeddingGenerationOptions> embeddingOptions = services.AddOptions<EmbeddingGenerationOptions>();
            OptionsBuilder<WorkspaceContextSearchOptions> searchOptions = services
                .AddOptions<WorkspaceContextSearchOptions>()
                .Validate(options => options.IsValid(), "Workspace context search weights must be non-negative, sum to 1, and use a positive recency half-life.")
                .ValidateOnStart();

            if (configuration is not null)
            {
                embeddingOptions.Bind(configuration.GetSection(EmbeddingGenerationOptions.SectionName));
                searchOptions.Bind(configuration.GetSection(WorkspaceContextSearchOptions.SectionName));
            }

            services.AddHttpClient<IEmbeddingGeneratorService, OpenAiCompatibleEmbeddingGeneratorService>();
            services.AddRepositories();
        }
    }
}