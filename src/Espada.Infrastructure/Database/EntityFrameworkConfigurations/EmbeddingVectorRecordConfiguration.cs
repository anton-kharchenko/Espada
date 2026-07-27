using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class EmbeddingVectorRecordConfiguration : IEntityTypeConfiguration<EmbeddingVectorRecord>, IEntityTypeConfiguration<Espada.Db.Models.ChunkEmbeddingVectors>
{
    public void Configure(EntityTypeBuilder<EmbeddingVectorRecord> builder)
    {
        builder.ToTable(DbTableConstants.ChunkEmbeddingVectors, DbConstants.SchemaName);

        builder.HasKey(record => record.ChunkEmbeddingId);

        builder.Property(record => record.ChunkEmbeddingId)
            .HasColumnName("ChunkEmbeddingId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ChunkEmbeddingId.Create(value))
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(record => record.Vector)
            .HasColumnName("Vector")
            .HasColumnType(DbNumericColumnTypeConstants.Vector)
            .IsRequired();

        builder.HasOne<ChunkEmbedding>()
            .WithOne()
            .HasForeignKey<EmbeddingVectorRecord>(record => record.ChunkEmbeddingId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.ChunkEmbeddingVectors> builder)
    {
        builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
        builder
            .HasOne<Espada.Db.Models.ChunkEmbeddings>()
            .WithOne()
            .HasForeignKey<Espada.Db.Models.ChunkEmbeddingVectors>(model => model.ChunkEmbeddingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}