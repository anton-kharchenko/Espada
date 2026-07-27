using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class EmbeddingVectorRecordConfiguration : IEntityTypeConfiguration<Models.ChunkEmbeddingVectors>
{
    public void Configure(EntityTypeBuilder<Models.ChunkEmbeddingVectors> builder)
    {
        builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
        builder
            .HasOne<Models.ChunkEmbeddings>()
            .WithOne()
            .HasForeignKey<Models.ChunkEmbeddingVectors>(model => model.ChunkEmbeddingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}