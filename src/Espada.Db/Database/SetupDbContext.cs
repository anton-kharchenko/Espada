using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Espada.Db.Database
{
    public sealed class SetupDbContext(DbContextOptions<SetupDbContext> options) : DbContext(options)
    {
        public DbSet<Organizations> Organizations => Set<Organizations>();
        public DbSet<OrganizationMemberships> OrganizationMemberships => Set<OrganizationMemberships>();
        public DbSet<Workspaces> Workspaces => Set<Workspaces>();
        public DbSet<Projects> Projects => Set<Projects>();
        public DbSet<Tasks> Tasks => Set<Tasks>();
        public DbSet<Bindings> Bindings => Set<Bindings>();
        public DbSet<InstructionRules> InstructionRules => Set<InstructionRules>();
        public DbSet<PolicyRules> PolicyRules => Set<PolicyRules>();
        public DbSet<MemoryMetadataRecords> MemoryMetadata => Set<MemoryMetadataRecords>();
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
        public DbSet<OneTimeBootstrapCodes> OneTimeBootstrapCodes => Set<OneTimeBootstrapCodes>();
        public DbSet<AgentProfiles> AgentProfiles => Set<AgentProfiles>();
        public DbSet<AgentInstallations> AgentInstallations => Set<AgentInstallations>();
        public DbSet<Devices> Devices => Set<Devices>();
        public DbSet<AgentSessions> AgentSessions => Set<AgentSessions>();
        public DbSet<AgentSessionEvents> AgentSessionEvents => Set<AgentSessionEvents>();
        public DbSet<AgentApprovals> AgentApprovals => Set<AgentApprovals>();
        public DbSet<SyncEvents> SyncEvents => Set<SyncEvents>();
        public DbSet<SyncCursors> SyncCursors => Set<SyncCursors>();
        public DbSet<SyncConflicts> SyncConflicts => Set<SyncConflicts>();
        public DbSet<AgentVendorTypes> AgentVendorTypes => Set<AgentVendorTypes>();
        public DbSet<AgentSessionStatusTypes> AgentSessionStatusTypes => Set<AgentSessionStatusTypes>();
        public DbSet<AgentSessionEventTypes> AgentSessionEventTypes => Set<AgentSessionEventTypes>();
        public DbSet<AgentApprovalStatusTypes> AgentApprovalStatusTypes => Set<AgentApprovalStatusTypes>();
        public DbSet<SyncConflictStatusTypes> SyncConflictStatusTypes => Set<SyncConflictStatusTypes>();
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
            modelBuilder.UseOpenIddict<Guid>();
            modelBuilder.HasPostgresExtension(DbExtensionConstants.Vector);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SetupDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigureMigrationWarnings(DbContextOptionsBuilder options)
        {
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(
                    RelationalEventId.PendingModelChangesWarning));
        }

        private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder options)
        {
            options.MigrationsAssembly(typeof(SetupDbContext).Assembly.FullName);
            options.MigrationsHistoryTable("__EFMigrationsHistory", DbConstants.SchemaName);
            options.UseVector();
        }
    }
}