using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class EmbeddingVectorRecordConfiguration : IEntityTypeConfiguration<ChunkEmbeddingVectors>
    {
        public void Configure(EntityTypeBuilder<ChunkEmbeddingVectors> builder)
        {
            builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
            builder
                .HasOne<ChunkEmbeddings>()
                .WithOne()
                .HasForeignKey<ChunkEmbeddingVectors>(model => model.ChunkEmbeddingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}