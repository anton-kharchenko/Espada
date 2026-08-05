using Espada.Application.Contracts.Persistence;
using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Database.EntityFrameworkConfigurations;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Database
{
    public sealed class EspadaDbContext(DbContextOptions<EspadaDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
        public DbSet<Binding> Bindings => Set<Binding>();
        public DbSet<InstructionRule> InstructionRules => Set<InstructionRule>();
        public DbSet<PolicyRule> PolicyRules => Set<PolicyRule>();
        public DbSet<MemoryMetadata> MemoryMetadata => Set<MemoryMetadata>();
        public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();

        public DbSet<AgentProfile> AgentProfiles => Set<AgentProfile>();

        public DbSet<AgentInstallation> AgentInstallations => Set<AgentInstallation>();

        public DbSet<Device> Devices => Set<Device>();

        public DbSet<AgentSession> AgentSessions => Set<AgentSession>();

        public DbSet<AgentSessionEvent> AgentSessionEvents => Set<AgentSessionEvent>();

        public DbSet<AgentApproval> AgentApprovals => Set<AgentApproval>();

        public DbSet<SyncEvent> SyncEvents => Set<SyncEvent>();

        internal DbSet<SyncDeviceRegistrations> SyncDeviceRegistrations => Set<SyncDeviceRegistrations>();

        public DbSet<SyncCursor> SyncCursors => Set<SyncCursor>();

        public DbSet<SyncConflict> SyncConflicts => Set<SyncConflict>();

        public DbSet<Source> Sources => Set<Source>();

        public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

        public DbSet<Artifact> Artifacts => Set<Artifact>();

        public DbSet<ArtifactRevision> ArtifactRevisions => Set<ArtifactRevision>();

        public DbSet<ChunkBatch> ChunkBatches => Set<ChunkBatch>();

        public DbSet<Chunk> Chunks => Set<Chunk>();

        public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();

        internal DbSet<EmbeddingVectorRecord> EmbeddingVectors => Set<EmbeddingVectorRecord>();

        internal DbSet<RepositoryManifestEntries> RepositoryManifestEntries => Set<RepositoryManifestEntries>();

        internal DbSet<OutboxMessageRecord> OutboxMessages => Set<OutboxMessageRecord>();

        internal DbSet<IngestionJobs> IngestionJobs => Set<IngestionJobs>();

        internal DbSet<BillingCustomers> BillingCustomers => Set<BillingCustomers>();

        internal DbSet<PaymentEvents> PaymentEvents => Set<PaymentEvents>();

        internal DbSet<UsageLedgerEntries> UsageLedgerEntries => Set<UsageLedgerEntries>();

        internal DbSet<UsageReconciliationOutbox> UsageReconciliationOutbox => Set<UsageReconciliationOutbox>();

        internal DbSet<OneTimeBootstrapCodes> OneTimeBootstrapCodes => Set<OneTimeBootstrapCodes>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IHasDomainEvents[] aggregates = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(entry => entry.Entity.DomainEvents.Count > 0)
                .Select(entry => entry.Entity)
                .ToArray();

            IDomainEvent[] events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToArray();
            DateTimeOffset occurredAtUtc = DateTimeOffset.UtcNow;
            OutboxMessageRecord[] messages = events
                .Select(domainEvent =>
                {
                    (string name, int version, string payload) = DomainEventSerializer.Serialize(domainEvent);
                    return new OutboxMessageRecord(Guid.NewGuid(), name, version, payload, occurredAtUtc);
                })
                .ToArray();

            OutboxMessages.AddRange(messages);

            try
            {
                int saved = await base.SaveChangesAsync(cancellationToken);
                foreach (IHasDomainEvents aggregate in aggregates)
                {
                    aggregate.DequeueDomainEvents();
                }

                return saved;
            }
            catch
            {
                foreach (OutboxMessageRecord message in messages)
                {
                    Entry(message).State = EntityState.Detached;
                }

                throw;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(DbConstants.SchemaName);
            modelBuilder.UseOpenIddict<Guid>();
            modelBuilder.HasPostgresExtension(DbExtensionConstants.Vector);
            modelBuilder.Ignore<IDomainEvent>();

            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationMembershipConfiguration());
            modelBuilder.ApplyConfiguration<Workspace>(new WorkspaceConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new TaskConfiguration());
            modelBuilder.ApplyConfiguration(new BindingConfiguration());
            modelBuilder.ApplyConfiguration(new InstructionRuleConfiguration());
            modelBuilder.ApplyConfiguration(new PolicyRuleConfiguration());
            modelBuilder.ApplyConfiguration(new MemoryMetadataConfiguration());
            modelBuilder.ApplyConfiguration<WorkspaceMembership>(new WorkspaceMembershipConfiguration());
            modelBuilder.ApplyConfiguration(new AgentProfileConfiguration());
            modelBuilder.ApplyConfiguration(new AgentInstallationConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new AgentSessionConfiguration());
            modelBuilder.ApplyConfiguration(new AgentSessionEventConfiguration());
            modelBuilder.ApplyConfiguration(new AgentApprovalConfiguration());
            modelBuilder.ApplyConfiguration(new SyncEventConfiguration());
            modelBuilder.ApplyConfiguration(new SyncDeviceRegistrationConfiguration());
            modelBuilder.ApplyConfiguration(new SyncCursorConfiguration());
            modelBuilder.ApplyConfiguration(new SyncConflictConfiguration());
            modelBuilder.ApplyConfiguration<Source>(new SourceConfiguration());
            modelBuilder.ApplyConfiguration<ImportJob>(new ImportJobConfiguration());
            modelBuilder.ApplyConfiguration<Artifact>(new ArtifactConfiguration());
            modelBuilder.ApplyConfiguration<ArtifactRevision>(new ArtifactRevisionConfiguration());
            modelBuilder.ApplyConfiguration<ChunkBatch>(new ChunkBatchConfiguration());
            modelBuilder.ApplyConfiguration<Chunk>(new ChunkConfiguration());
            modelBuilder.ApplyConfiguration<ChunkEmbedding>(new ChunkEmbeddingConfiguration());
            modelBuilder.ApplyConfiguration<EmbeddingVectorRecord>(new EmbeddingVectorRecordConfiguration());
            modelBuilder.ApplyConfiguration(new RepositoryManifestEntryConfiguration());
            modelBuilder.ApplyConfiguration(new IngestionJobConfiguration());
            modelBuilder.ApplyConfiguration<OutboxMessageRecord>(new OutboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new BillingCustomerConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentEventConfiguration());
            modelBuilder.ApplyConfiguration(new UsageLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new UsageReconciliationConfiguration());
            modelBuilder.ApplyConfiguration(new OneTimeBootstrapCodeConfiguration());
        }
    }
}