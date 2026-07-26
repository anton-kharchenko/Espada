using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ArtifactRevisionConfiguration : IEntityTypeConfiguration<ArtifactRevision>, IEntityTypeConfiguration<Espada.Db.Models.ArtifactRevisions>
{
    public void Configure(EntityTypeBuilder<ArtifactRevision> builder)
    {
        builder.ToTable(DbConstants.Tables.ArtifactRevisions, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ArtifactRevisionId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactId)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ArtifactId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Number)
            .HasColumnName("RevisionNumber")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(number => number.Value, value => RevisionNumber.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Content)
            .HasColumnName("Content")
            .HasColumnType(DbConstants.ColumnTypes.Text.TextType)
            .HasConversion(content => content.Value, value => ArtifactContent.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Artifact>()
            .WithMany()
            .HasForeignKey(e => e.ArtifactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ArtifactId)
            .HasDatabaseName("IX_ArtifactRevisions_ArtifactId");

        builder.HasIndex(e => new { e.ArtifactId, e.Number })
            .IsUnique()
            .HasDatabaseName(DbConstants.Indexes.ArtifactRevisionArtifactNumber);

        builder.Ignore(e => e.ContentHash);
        builder.Ignore(e => e.SizeInBytes);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.ArtifactRevisions> builder)
    {
        builder.Property(model => model.ArtifactRevisionId).ValueGeneratedNever();
        builder.HasOne<Espada.Db.Models.Artifacts>().WithMany().HasForeignKey(model => model.ArtifactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ArtifactRevisions_ArtifactId");
        builder.HasIndex(model => new { model.ArtifactId, model.RevisionNumber }).IsUnique().HasDatabaseName(DbConstants.Indexes.ArtifactRevisionArtifactNumber);
    }
}