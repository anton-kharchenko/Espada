using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Infrastructure.Database.EntityFrameworkConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Database
{
    internal sealed class WorkspaceContextSearchDbContext(DbContextOptions<WorkspaceContextSearchDbContext> options)
        : DbContext(options)
    {
        public DbSet<ChunkEmbeddingVectors> EmbeddingVectors => Set<ChunkEmbeddingVectors>();

        public DbSet<ChunkEmbeddings> ChunkEmbeddings => Set<ChunkEmbeddings>();

        public DbSet<Chunks> Chunks => Set<Chunks>();

        public DbSet<Artifacts> Artifacts => Set<Artifacts>();

        public DbSet<ArtifactRevisions> ArtifactRevisions => Set<ArtifactRevisions>();

        public DbSet<ImportJobs> ImportJobs => Set<ImportJobs>();

        public DbSet<Sources> Sources => Set<Sources>();

        public DbSet<MemoryMetadataRecords> MemoryMetadata => Set<MemoryMetadataRecords>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DbConstants.SchemaName);
            modelBuilder.HasPostgresExtension(DbExtensionConstants.Vector);

            new ImportJobConfiguration().Configure(modelBuilder.Entity<ImportJobs>());

            modelBuilder.Entity<Chunks>().OwnsOne(chunk => chunk.SourceSpan, span =>
            {
                span.Property(sourceSpan => sourceSpan.Start).HasColumnName("SourceStart");
                span.Property(sourceSpan => sourceSpan.Length).HasColumnName("SourceLength");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}