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

    public DbSet<Source> Sources => Set<Source>();

    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    public DbSet<Artifact> Artifacts => Set<Artifact>();

    public DbSet<ArtifactRevision> ArtifactRevisions => Set<ArtifactRevision>();

    public DbSet<ChunkBatch> ChunkBatches => Set<ChunkBatch>();

    public DbSet<Chunk> Chunks => Set<Chunk>();

    public DbSet<ChunkEmbedding> ChunkEmbeddings => Set<ChunkEmbedding>();

    internal DbSet<EmbeddingVectorRecord> EmbeddingVectors => Set<EmbeddingVectorRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension(DbConstants.Extensions.Vector);
        modelBuilder.Ignore<IDomainEvent>();

        modelBuilder.ApplyConfiguration<Workspace>(new WorkspaceConfiguration());
        modelBuilder.ApplyConfiguration<Source>(new SourceConfiguration());
        modelBuilder.ApplyConfiguration<ImportJob>(new ImportJobConfiguration());
        modelBuilder.ApplyConfiguration<Artifact>(new ArtifactConfiguration());
        modelBuilder.ApplyConfiguration<ArtifactRevision>(new ArtifactRevisionConfiguration());
        modelBuilder.ApplyConfiguration<ChunkBatch>(new ChunkBatchConfiguration());
        modelBuilder.ApplyConfiguration<Chunk>(new ChunkConfiguration());
        modelBuilder.ApplyConfiguration<ChunkEmbedding>(new ChunkEmbeddingConfiguration());
        modelBuilder.ApplyConfiguration<EmbeddingVectorRecord>(new EmbeddingVectorRecordConfiguration());
    }
}