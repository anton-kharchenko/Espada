using Espada.Application.Contracts.Persistence;
using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Database.EntityFrameworkConfigurations;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Database;

public sealed class EspadaDbContext(DbContextOptions<EspadaDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();

    public DbSet<Source> Sources => Set<Source>();

    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    public DbSet<Artifact> Artifacts => Set<Artifact>();

    public DbSet<ArtifactRevision> ArtifactRevisions => Set<ArtifactRevision>();

    public DbSet<ChunkBatch> ChunkBatches => Set<ChunkBatch>();

    public DbSet<Chunk> Chunks => Set<Chunk>();

    public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();

    internal DbSet<EmbeddingVectorRecord> EmbeddingVectors => Set<EmbeddingVectorRecord>();

    internal DbSet<OutboxMessageRecord> OutboxMessages => Set<OutboxMessageRecord>();

    internal DbSet<Espada.Db.Models.IngestionJobs> IngestionJobs => Set<Espada.Db.Models.IngestionJobs>();

    internal DbSet<Espada.Db.Models.BillingCustomers> BillingCustomers => Set<Espada.Db.Models.BillingCustomers>();

    internal DbSet<Espada.Db.Models.PaymentEvents> PaymentEvents => Set<Espada.Db.Models.PaymentEvents>();

    internal DbSet<Espada.Db.Models.UsageLedgerEntries> UsageLedgerEntries => Set<Espada.Db.Models.UsageLedgerEntries>();

    internal DbSet<Espada.Db.Models.UsageReconciliationOutbox> UsageReconciliationOutbox => Set<Espada.Db.Models.UsageReconciliationOutbox>();

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

        modelBuilder.HasPostgresExtension(DbExtensionConstants.Vector);
        modelBuilder.Ignore<IDomainEvent>();

        modelBuilder.ApplyConfiguration<Workspace>(new WorkspaceConfiguration());
        modelBuilder.ApplyConfiguration<WorkspaceMembership>(new WorkspaceMembershipConfiguration());
        modelBuilder.ApplyConfiguration<Source>(new SourceConfiguration());
        modelBuilder.ApplyConfiguration<ImportJob>(new ImportJobConfiguration());
        modelBuilder.ApplyConfiguration<Artifact>(new ArtifactConfiguration());
        modelBuilder.ApplyConfiguration<ArtifactRevision>(new ArtifactRevisionConfiguration());
        modelBuilder.ApplyConfiguration<ChunkBatch>(new ChunkBatchConfiguration());
        modelBuilder.ApplyConfiguration<Chunk>(new ChunkConfiguration());
        modelBuilder.ApplyConfiguration<ChunkEmbedding>(new ChunkEmbeddingConfiguration());
        modelBuilder.ApplyConfiguration<EmbeddingVectorRecord>(new EmbeddingVectorRecordConfiguration());
        modelBuilder.ApplyConfiguration(new IngestionJobConfiguration());
        modelBuilder.ApplyConfiguration<OutboxMessageRecord>(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new BillingCustomerConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentEventConfiguration());
        modelBuilder.ApplyConfiguration(new UsageLedgerConfiguration());
        modelBuilder.ApplyConfiguration(new UsageReconciliationConfiguration());
    }
}