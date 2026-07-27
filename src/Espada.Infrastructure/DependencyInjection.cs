using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Db.Constants;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Extensions;
using Espada.Infrastructure.Options;
using Espada.Infrastructure.Repositories;
using Espada.Infrastructure.Services;
using Espada.Application.Contracts.Jobs;
using Espada.Infrastructure.Jobs;
using Espada.Domain.SeedWork;
using Espada.Application.Contracts.Blobs;
using Espada.Application.Contracts.Ingestion;
using Espada.Infrastructure.Blobs;
using Espada.Infrastructure.Ingestion;
using Espada.Infrastructure.Ingestion.Chunking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Espada.Billing;
using Espada.Infrastructure.Billing;
using Espada.Application.Contracts.Billing;
using Espada.Billing.Contracts;
using Espada.Infrastructure.Options.Constants;

namespace Espada.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration? configuration = null)
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
                    npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        DbConstants.SchemaName);
                    npgsqlOptions.UseVector();
                }));
        services.AddDbContext<WorkspaceContextSearchDbContext>((serviceProvider, options) =>
            options.UseNpgsql(
                serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                npgsqlOptions => npgsqlOptions.UseVector())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<EspadaDbContext>());
        services.AddScoped<IEmbeddingVectorStore, EmbeddingVectorStore>();
        services.AddScoped<
            IWorkspaceContextSearchStore,
            WorkspaceContextSearchStore>();
        services.AddScoped<IJobQueue, PostgreSqlJobQueue>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IOutboxPublisher, PostgreSqlOutboxPublisher>();
        services.AddScoped<IBillingStore, PostgreSqlBillingStore>();
        services.AddScoped<IUsageMeter, PostgreSqlUsageMeter>();

        OptionsBuilder<EmbeddingGenerationOptions> embeddingOptions =
            services.AddOptions<EmbeddingGenerationOptions>();
        OptionsBuilder<IngestionOptions> ingestionOptions = services
            .AddOptions<IngestionOptions>()
            .Validate(options => options.IsValid(), "Ingestion limits must be positive.")
            .ValidateOnStart();
        services.AddOptions<BlobStorageOptions>();
        services.AddOptions<ConnectorRuntimeOptions>();
        OptionsBuilder<WorkspaceContextSearchOptions> searchOptions = services
            .AddOptions<WorkspaceContextSearchOptions>()
            .Validate(
                options => options.IsValid(),
                "Workspace context search weights must be non-negative, sum to 1, and use a positive recency half-life.")
            .ValidateOnStart();

        if (configuration is not null)
        {
            embeddingOptions.Bind(
                configuration.GetSection(
            EmbeddingGenerationConstants.SectionName));
            ingestionOptions.Bind(
            configuration.GetSection(IngestionConstants.SectionName));
            services.Configure<BlobStorageOptions>(
            configuration.GetSection(BlobStorageConstants.SectionName));
            services.Configure<ConnectorRuntimeOptions>(
            configuration.GetSection(ConnectorRuntimeConstants.SectionName));
            searchOptions.Bind(
                configuration.GetSection(
                    WorkspaceContextSearchOptions.SectionName));
        }

        services.AddHttpClient<OpenAiCompatibleEmbeddingGeneratorService>();
        services.AddTransient<IEmbeddingGeneratorService>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OpenAiCompatibleEmbeddingGeneratorService>());
        services.AddTransient<IBatchEmbeddingGeneratorService>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OpenAiCompatibleEmbeddingGeneratorService>());
        services.AddSingleton<IBlobStore>(serviceProvider =>
        {
            IngestionOptions options = serviceProvider
                .GetRequiredService<IOptions<IngestionOptions>>()
                .Value;
            BlobStorageOptions blobOptions = serviceProvider
                .GetRequiredService<IOptions<BlobStorageOptions>>()
                .Value;
            if (blobOptions.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(blobOptions.AzureContainerUri, UriKind.Absolute, out Uri? containerUri)
                    || !containerUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "BlobStorage:AzureContainerUri must be an absolute HTTPS URI.");
                }

                return new AzureBlobStore(containerUri);
            }

            string root = string.IsNullOrWhiteSpace(options.BlobRoot)
                ? Path.Join(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "Espada",
                    "blobs")
                : options.BlobRoot;
            return new FileSystemBlobStore(root);
        });
        services.AddTransient<ISourceReader, SourceReader>();
        services.AddTransient<IConnectorSourceClient, ApprovedMcpConnectorSourceClient>();
        services.AddTransient<ISourceParser, SourceParser>();
        services.AddTransient<IChunkingStrategy, FixedSizeChunkingStrategy>();
        services.AddTransient<IChunkingStrategy, RecursiveChunkingStrategy>();
        services.AddTransient<IChunkingStrategy, MarkdownChunkingStrategy>();
        services.AddTransient<IChunkingStrategy, CodeChunkingStrategy>();
        services.AddTransient<IChunkingStrategy, SemanticChunkingStrategy>();
        services.AddTransient<IChunkingStrategy, CustomChunkingStrategy>();
        services.AddRepositories();
    }
}