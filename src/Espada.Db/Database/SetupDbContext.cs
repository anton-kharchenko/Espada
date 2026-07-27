using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Espada.Db.Database;

public sealed class SetupDbContext(DbContextOptions<SetupDbContext> options) : DbContext(options)
{
    private const string InfrastructureAssemblyName = "Espada.Infrastructure";

    public DbSet<Workspaces> Workspaces => Set<Workspaces>();
    public DbSet<Sources> Sources => Set<Sources>();
    public DbSet<ImportJobs> ImportJobs => Set<ImportJobs>();
    public DbSet<Artifacts> Artifacts => Set<Artifacts>();
    public DbSet<ArtifactRevisions> ArtifactRevisions => Set<ArtifactRevisions>();
    public DbSet<ChunkBatches> ChunkBatches => Set<ChunkBatches>();
    public DbSet<Chunks> Chunks => Set<Chunks>();
    public DbSet<ChunkEmbeddings> ChunkEmbeddings => Set<ChunkEmbeddings>();
    public DbSet<ChunkEmbeddingVectors> EmbeddingVectors => Set<ChunkEmbeddingVectors>();
    public DbSet<IngestionJobs> IngestionJobs => Set<IngestionJobs>();
    public DbSet<OutboxMessages> OutboxMessages => Set<OutboxMessages>();
    public DbSet<WorkspaceMemberships> WorkspaceMemberships => Set<WorkspaceMemberships>();
    public DbSet<BillingCustomers> BillingCustomers => Set<BillingCustomers>();
    public DbSet<PaymentEvents> PaymentEvents => Set<PaymentEvents>();
    public DbSet<UsageLedgerEntries> UsageLedgerEntries => Set<UsageLedgerEntries>();
    public DbSet<UsageReconciliationOutbox> UsageReconciliationOutbox => Set<UsageReconciliationOutbox>();
    public DbSet<WorkspaceTypes> WorkspaceTypes => Set<WorkspaceTypes>();
    public DbSet<WorkspaceStatusTypes> WorkspaceStatusTypes => Set<WorkspaceStatusTypes>();
    public DbSet<SourceTypes> SourceTypes => Set<SourceTypes>();
    public DbSet<SourceStatusTypes> SourceStatusTypes => Set<SourceStatusTypes>();
    public DbSet<ImportStatusTypes> ImportStatusTypes => Set<ImportStatusTypes>();
    public DbSet<ArtifactTypes> ArtifactTypes => Set<ArtifactTypes>();
    public DbSet<ArtifactStatusTypes> ArtifactStatusTypes => Set<ArtifactStatusTypes>();
    public DbSet<ChunkingStrategyTypes> ChunkingStrategyTypes => Set<ChunkingStrategyTypes>();
    public DbSet<ChunkBatchStatusTypes> ChunkBatchStatusTypes => Set<ChunkBatchStatusTypes>();

    internal static DatabaseRuntime CreateRuntime(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString =
            Environment.GetEnvironmentVariable(
                DbConstants.ConnectionStringEnvironmentVariable)
            ?? configuration.GetConnectionString(DbConstants.ConnectionString)
            ?? throw new InvalidOperationException(
                "Database connection string was not configured. Set "
                + $"ConnectionStrings:{DbConstants.ConnectionString} or "
                + $"{DbConstants.ConnectionStringEnvironmentVariable}.");

        NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
        dataSourceBuilder.UseVector();
        NpgsqlDataSource dataSource = dataSourceBuilder.Build();
        DbContextOptionsBuilder<SetupDbContext> options = new();
        ConfigureMigrationWarnings(options);
        options.UseNpgsql(dataSource, ConfigureNpgsql);
        return new DatabaseRuntime(new SetupDbContext(options.Options), dataSource);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DbConstants.SchemaName);
        modelBuilder.HasPostgresExtension(DbExtensionConstants.Vector);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SetupDbContext).Assembly);

        try
        {
            System.Reflection.Assembly infrastructureAssembly =
                AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == InfrastructureAssemblyName)
                ?? System.Reflection.Assembly.Load(InfrastructureAssemblyName);

            modelBuilder.ApplyConfigurationsFromAssembly(infrastructureAssembly);
        }
        catch (FileNotFoundException)
        {
            // Espada.Db can apply compiled migrations without loading the application infrastructure.
        }

        foreach (
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType
            in modelBuilder.Model
                .GetEntityTypes()
                .Where(
                    entityType =>
                        entityType.ClrType.Assembly
                        != typeof(Workspaces).Assembly)
                .ToList())
        {
            modelBuilder.Ignore(entityType.ClrType);
            modelBuilder.Model.RemoveEntityType(entityType.ClrType);
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureMigrationWarnings(DbContextOptionsBuilder options) =>
        options.ConfigureWarnings(
            warnings =>
                warnings.Ignore(
                    RelationalEventId.PendingModelChangesWarning));

    private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder options)
    {
        options.MigrationsAssembly(typeof(SetupDbContext).Assembly.FullName);
        options.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
        options.UseVector();
    }
}