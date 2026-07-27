using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ChunkBatchConfiguration : IEntityTypeConfiguration<Models.ChunkBatches>
{
    public void Configure(EntityTypeBuilder<Models.ChunkBatches> builder)
    {
        builder.Property(model => model.ChunkBatchId).ValueGeneratedNever();
        builder.Property(model => model.Version).IsRowVersion();
        builder.HasOne<Models.ArtifactRevisions>().WithMany().HasForeignKey(model => model.ArtifactRevisionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.ChunkingStrategyTypes>().WithMany().HasForeignKey(model => model.StrategyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.ChunkBatchStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ChunkBatches_WorkspaceId");
        builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ChunkBatches_ArtifactId");
        builder.HasIndex(model => model.ArtifactRevisionId).HasDatabaseName("IX_ChunkBatches_ArtifactRevisionId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_ChunkBatches_StatusId");
        builder
            .HasIndex(
                model => new
                {
                    model.ArtifactRevisionId,
                    model.StrategyId,
                    model.StrategyVersion
                })
            .HasDatabaseName(
                "IX_ChunkBatches_Revision_Strategy_Version");
    }
}
