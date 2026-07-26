using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ArtifactConfiguration : IEntityTypeConfiguration<Artifact>, IEntityTypeConfiguration<Espada.Db.Models.Artifacts>
{
    public void Configure(EntityTypeBuilder<Artifact> builder)
    {
        ValueConverter<ArtifactRevisionId?, Guid?> revisionIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ArtifactRevisionId.Create(value.Value));

        ValueConverter<RevisionNumber?, int?> revisionNumberConverter = new(
            number => number == null ? null : number.Value,
            value => value == null ? null : RevisionNumber.Create(value.Value).Value!);

        builder.ToTable(DbConstants.Tables.Artifacts, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ArtifactId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Title)
            .HasColumnName("Title")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar200)
            .HasConversion(title => title.Value, value => ArtifactTitle.Create(value).Value!)
            .HasMaxLength(DbConstants.Validations.MaxLengths.L200)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Type)
            .HasColumnName("TypeId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(type => type.Id, value => Enumeration.GetAll<ArtifactType>().Single(type => type.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<ArtifactStatusType>().Single(status => status.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CurrentRevisionId)
            .HasColumnName("CurrentRevisionId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(revisionIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CurrentRevisionNumber)
            .HasColumnName("CurrentRevisionNumber")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(revisionNumberConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("UpdatedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArchivedAtUtc)
            .HasColumnName("ArchivedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Version)
            .HasColumnName("Version")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.BigInt)
            .HasDefaultValue(1L)
            .IsConcurrencyToken()
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(e => e.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_Artifacts_WorkspaceId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Artifacts_StatusId");

        builder.HasIndex(e => new { e.WorkspaceId, e.Title })
            .HasDatabaseName("IX_Artifacts_WorkspaceId_Title");

        builder.Ignore(e => e.RevisionCount);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.Artifacts> builder)
    {
        builder.Property(model => model.ArtifactId).ValueGeneratedNever();
        builder.Property(model => model.Version).HasDefaultValue(1L).IsConcurrencyToken();
        builder.HasOne<Espada.Db.Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ArtifactTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ArtifactStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Artifacts_WorkspaceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Artifacts_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.Title }).HasDatabaseName("IX_Artifacts_WorkspaceId_Title");
    }
}