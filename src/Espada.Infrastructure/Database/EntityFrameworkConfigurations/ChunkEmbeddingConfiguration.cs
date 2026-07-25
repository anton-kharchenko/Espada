using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkEmbeddingConfiguration : IEntityTypeConfiguration<ChunkEmbedding>
{
    public void Configure(EntityTypeBuilder<ChunkEmbedding> builder)
    {
        builder.ToTable(DbConstants.Tables.ChunkEmbeddings, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ChunkEmbeddingId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ChunkEmbeddingId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ChunkId)
            .HasColumnName("ChunkId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ChunkId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ChunkContentHash)
            .HasColumnName("ChunkContentHash")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar64)
            .HasConversion(hash => hash.Value, value => ContentHash.Create(value))
            .HasMaxLength(DbConstants.Validations.MaxLengths.L64)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(e => e.Model, model =>
        {
            model.Property(e => e.Identifier)
                .HasColumnName("ModelIdentifier")
                .HasColumnType(DbConstants.ColumnTypes.Text.Varchar200)
                .HasMaxLength(DbConstants.Validations.MaxLengths.L200)
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            model.Property(e => e.Version)
                .HasColumnName("ModelVersion")
                .HasColumnType(DbConstants.ColumnTypes.Text.Varchar100)
                .HasMaxLength(DbConstants.Validations.MaxLengths.L100)
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(e => e.Model)
            .IsRequired();

        builder.Property(e => e.Dimensions)
            .HasColumnName("Dimensions")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(dimensions => dimensions.Value, value => EmbeddingDimensions.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_ChunkEmbeddings_WorkspaceId");

        builder.HasIndex(e => e.ChunkId)
            .HasDatabaseName("IX_ChunkEmbeddings_ChunkId");

        builder.Ignore(e => e.DomainEvents);
    }
}