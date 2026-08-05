using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class MemoryMetadataConfiguration : IEntityTypeConfiguration<MemoryMetadata>
    {
        public void Configure(EntityTypeBuilder<MemoryMetadata> builder)
        {
            ValueConverter<MemoryId?, Guid?> supersededIdConverter = new(id => id == null ? null : id.Value,
                value => value == null ? null : MemoryId.Create(value.Value));
            builder.ToTable(DbTableConstants.MemoryMetadata, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.MemoryMetadataKind, "\"Kind\" = 'memory'"));
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("MemoryId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => MemoryId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.ArtifactId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactId.Create(value)).IsRequired();
            builder.Property(entity => entity.ArtifactRevisionId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value)).IsRequired();
            builder.Property(entity => entity.KindType).HasColumnName("Kind")
                .HasColumnType(DbTextColumnTypeConstants.Varchar32).HasMaxLength(DbMaxLengthConstants.L32)
                .HasConversion(kind => kind.Name,
                    value => Enumeration.GetAll<ArtifactKindType>().Single(kind => kind.Name == value)).IsRequired();
            builder.Property(entity => entity.CategoryType).HasColumnName("Category")
                .HasColumnType(DbTextColumnTypeConstants.Varchar32).HasMaxLength(DbMaxLengthConstants.L32)
                .HasConversion(category => category.Name,
                    value => Enumeration.GetAll<MemoryCategoryType>().Single(category => category.Name == value))
                .IsRequired();
            builder.Property(entity => entity.Confidence).HasColumnType(DbNumericColumnTypeConstants.Numeric5_4)
                .HasPrecision(5, 4).IsRequired();
            builder.Property(entity => entity.UserConfirmed).HasColumnType("boolean").IsRequired();
            builder.Property(entity => entity.ClientIdentity).HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200).IsRequired();
            builder.Property(entity => entity.SessionIdentity).HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200);
            builder.Property(entity => entity.CapturedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.SupersededMemoryId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(supersededIdConverter);
            builder.HasOne<ArtifactRevision>().WithMany()
                .HasForeignKey(entity => new { entity.ArtifactRevisionId, entity.ArtifactId, Kind = entity.KindType })
                .HasPrincipalKey(revision => new { revision.Id, revision.ArtifactId, Kind = revision.KindType })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<MemoryMetadata>().WithMany().HasForeignKey(entity => entity.SupersededMemoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.ArtifactId, entity.ArtifactRevisionId }).IsUnique()
                .HasDatabaseName("UX_MemoryMetadata_ArtifactId_ArtifactRevisionId");
            builder.HasIndex(entity => entity.SupersededMemoryId).IsUnique()
                .HasFilter("\"SupersededMemoryId\" IS NOT NULL")
                .HasDatabaseName(DbIndexConstants.MemoryMetadataSupersededMemory);
        }
    }
}