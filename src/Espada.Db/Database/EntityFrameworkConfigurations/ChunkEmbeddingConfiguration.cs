using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class ChunkEmbeddingConfiguration : IEntityTypeConfiguration<ChunkEmbeddings>
    {
        public void Configure(EntityTypeBuilder<ChunkEmbeddings> builder)
        {
            builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
            builder.HasOne<Chunks>().WithMany().HasForeignKey(model => model.ChunkId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ChunkEmbeddings_WorkspaceId");
            builder.HasIndex(model => model.ChunkId).HasDatabaseName("IX_ChunkEmbeddings_ChunkId");
            builder
                .HasIndex(model => new { model.ChunkId, model.ModelIdentifier, model.ModelVersion })
                .IsUnique()
                .HasDatabaseName(
                    DbIndexConstants.ChunkEmbeddingChunkModel);
        }
    }
}