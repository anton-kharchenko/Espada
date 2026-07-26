using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Espada.Infrastructure.Database
{
    public sealed class EspadaDbContext(DbContextOptions<EspadaDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<Workspace> Workspaces => Set<Workspace>();

        public DbSet<Source> Sources => Set<Source>();

        public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

        public DbSet<Artifact> Artifacts => Set<Artifact>();

        public DbSet<ArtifactRevision> ArtifactRevisions => Set<ArtifactRevision>();

        public DbSet<ChunkBatch> ChunkBatches => Set<ChunkBatch>();

        public DbSet<Chunk> Chunks => Set<Chunk>();

        public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();

        internal DbSet<EmbeddingVectorRecord> EmbeddingVectors => Set<EmbeddingVectorRecord>();

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IncrementConcurrencyVersions();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            IncrementConcurrencyVersions();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void IncrementConcurrencyVersions()
        {
            foreach (EntityEntry<IHasConcurrencyVersion> entry in ChangeTracker.Entries<IHasConcurrencyVersion>().Where(entry => entry.State == EntityState.Modified))
            {
                PropertyEntry<IHasConcurrencyVersion, long> version = entry.Property<long>(nameof(IHasConcurrencyVersion.Version));
                version.CurrentValue = version.OriginalValue + 1;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EspadaDbContext).Assembly);
            RemoveSetupModels(modelBuilder);
            IgnoreDomainEvents(modelBuilder);
        }

        private static void RemoveSetupModels(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes().Where(entityType => entityType.ClrType.Assembly == typeof(Db.Models.Workspaces).Assembly).ToList())
            {
                modelBuilder.Ignore(entityType.ClrType);
                modelBuilder.Model.RemoveEntityType(entityType.ClrType);
            }
        }

        private static void IgnoreDomainEvents(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvents.DomainEvents));
            }
        }
    }
}