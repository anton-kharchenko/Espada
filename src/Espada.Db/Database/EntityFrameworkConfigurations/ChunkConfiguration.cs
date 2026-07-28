using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class ChunkConfiguration : IEntityTypeConfiguration<Chunks>
    {
        public void Configure(EntityTypeBuilder<Chunks> builder)
        {
            builder.Property(model => model.ChunkId).ValueGeneratedNever();
            builder.OwnsOne(model => model.SourceSpan, span =>
            {
                span.Property(model => model.Start).HasColumnName("SourceStart")
                    .HasColumnType(DbNumericColumnTypeConstants.Integer);
                span.Property(model => model.Length).HasColumnName("SourceLength")
                    .HasColumnType(DbNumericColumnTypeConstants.Integer);
            });
            builder.Navigation(model => model.SourceSpan).IsRequired(false);
            builder.HasOne<ChunkBatches>().WithMany().HasForeignKey(model => model.ChunkBatchId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Artifacts>().WithMany().HasForeignKey(model => model.ArtifactId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ArtifactRevisions>().WithMany().HasForeignKey(model => model.ArtifactRevisionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ChunkingStrategyTypes>().WithMany().HasForeignKey(model => model.StrategyId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.ChunkBatchId).HasDatabaseName("IX_Chunks_ChunkBatchId");
            builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Chunks_WorkspaceId");
            builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_Chunks_ArtifactId");
            builder.HasIndex(model => model.ArtifactRevisionId).HasDatabaseName("IX_Chunks_ArtifactRevisionId");
            builder.HasIndex(model => new { model.ChunkBatchId, model.ChunkNumber }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ChunkBatchNumber);
        }
    }
}