using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ChunkEmbeddingConfiguration : IEntityTypeConfiguration<Models.ChunkEmbeddings>
{
    public void Configure(EntityTypeBuilder<Models.ChunkEmbeddings> builder)
    {
        builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
        builder.HasOne<Models.Chunks>().WithMany().HasForeignKey(model => model.ChunkId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ChunkEmbeddings_WorkspaceId");
        builder.HasIndex(model => model.ChunkId).HasDatabaseName("IX_ChunkEmbeddings_ChunkId");
        builder
            .HasIndex(
                model => new
                {
                    model.ChunkId,
                    model.ModelIdentifier,
                    model.ModelVersion
                })
            .IsUnique()
            .HasDatabaseName(
                DbIndexConstants.ChunkEmbeddingChunkModel);
    }
}
