using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EspadaDbContext).Assembly);
        }

    }
}