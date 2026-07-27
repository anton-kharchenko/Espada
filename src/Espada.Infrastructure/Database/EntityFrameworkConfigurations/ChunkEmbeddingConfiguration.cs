using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkEmbeddingConfiguration : IEntityTypeConfiguration<ChunkEmbedding>, IEntityTypeConfiguration<Espada.Db.Models.ChunkEmbeddings>
{
    public void Configure(EntityTypeBuilder<ChunkEmbedding> builder)
    {
        builder.ToTable(DbTableConstants.ChunkEmbeddings, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ChunkEmbeddingId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ChunkEmbeddingId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ChunkId)
            .HasColumnName("ChunkId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ChunkId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ChunkContentHash)
            .HasColumnName("ChunkContentHash")
            .HasColumnType(DbTextColumnTypeConstants.Varchar64)
            .HasConversion(hash => hash.Value, value => ContentHash.Create(value))
            .HasMaxLength(DbMaxLengthConstants.L64)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(e => e.Model);

        builder.Property<string>(DbPropertyConstants.ChunkEmbeddingModelIdentifier)
            .HasField(DbPropertyConstants.ChunkEmbeddingModelIdentifier)
            .HasColumnName("ModelIdentifier")
            .HasColumnType(DbTextColumnTypeConstants.Varchar200)
            .HasMaxLength(DbMaxLengthConstants.L200)
            .IsRequired();

        builder.Property<string>(DbPropertyConstants.ChunkEmbeddingModelVersion)
            .HasField(DbPropertyConstants.ChunkEmbeddingModelVersion)
            .HasColumnName("ModelVersion")
            .HasColumnType(DbTextColumnTypeConstants.Varchar100)
            .HasMaxLength(DbMaxLengthConstants.L100)
            .IsRequired();
        builder.Property(e => e.Dimensions)
            .HasColumnName("Dimensions")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(dimensions => dimensions.Value, value => EmbeddingDimensions.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Chunk>()
            .WithMany()
            .HasForeignKey(e => e.ChunkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_ChunkEmbeddings_WorkspaceId");

        builder.HasIndex(e => e.ChunkId)
            .HasDatabaseName("IX_ChunkEmbeddings_ChunkId");

        builder.HasIndex(
            nameof(ChunkEmbedding.ChunkId),
            DbPropertyConstants.ChunkEmbeddingModelIdentifier,
            DbPropertyConstants.ChunkEmbeddingModelVersion)
            .IsUnique()
            .HasDatabaseName(DbIndexConstants.ChunkEmbeddingChunkModel);

        builder.Ignore(e => e.DomainEvents);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.ChunkEmbeddings> builder)
    {
        builder.Property(model => model.ChunkEmbeddingId).ValueGeneratedNever();
        builder.HasOne<Espada.Db.Models.Chunks>().WithMany().HasForeignKey(model => model.ChunkId).OnDelete(DeleteBehavior.Restrict);
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