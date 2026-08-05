using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class ArtifactRevisionConfiguration : IEntityTypeConfiguration<ArtifactRevision>,
        IEntityTypeConfiguration<ArtifactRevisions>
    {
        public void Configure(EntityTypeBuilder<ArtifactRevision> builder)
        {
            builder.ToTable(DbTableConstants.ArtifactRevisions, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("ArtifactRevisionId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value)).IsRequired()
                .ValueGeneratedNever().UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.ArtifactId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactId.Create(value)).IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.WorkspaceId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value)).IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.KindType).HasColumnName("Kind")
                .HasColumnType(DbTextColumnTypeConstants.Varchar32).HasMaxLength(DbMaxLengthConstants.L32)
                .HasConversion(kind => kind.Name,
                    value => Enumeration.GetAll<ArtifactKindType>().Single(kind => kind.Name == value)).IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.Number).HasColumnName("RevisionNumber")
                .HasColumnType(DbNumericColumnTypeConstants.Integer)
                .HasConversion(number => number.Value, value => RevisionNumber.Create(value).Value!).IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.Content).HasColumnType(DbTextColumnTypeConstants.TextType)
                .HasConversion(content => content.Value, value => ArtifactContent.Create(value).Value!).IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Property(entity => entity.CreatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasAlternateKey(entity => new { entity.Id, entity.WorkspaceId });
            builder.HasAlternateKey(entity => new { entity.Id, Kind = entity.KindType });
            builder.HasAlternateKey(entity => new { entity.Id, entity.ArtifactId, Kind = entity.KindType });
            builder.HasOne<Artifact>().WithMany().HasForeignKey(entity => new { entity.ArtifactId, entity.WorkspaceId })
                .HasPrincipalKey(artifact => new { artifact.Id, artifact.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => entity.ArtifactId).HasDatabaseName("IX_ArtifactRevisions_ArtifactId");
            builder.HasIndex(entity => new { entity.ArtifactId, entity.Number }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ArtifactRevisionArtifactNumber);
            builder.Ignore(entity => entity.ContentHash);
            builder.Ignore(entity => entity.SizeInBytes);
        }

        public void Configure(EntityTypeBuilder<ArtifactRevisions> builder)
        {
            builder.Property(model => model.ArtifactRevisionId).ValueGeneratedNever();
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.WorkspaceId });
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.Kind });
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.ArtifactId, model.Kind });
            builder.HasOne<Artifacts>().WithMany().HasForeignKey(model => new { model.ArtifactId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.ArtifactId, model.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ArtifactRevisions_ArtifactId");
            builder.HasIndex(model => new { model.ArtifactId, model.RevisionNumber }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ArtifactRevisionArtifactNumber);
        }
    }
}