using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>, IEntityTypeConfiguration<Espada.Db.Models.Sources>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable(DbConstants.Tables.Sources, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("SourceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => SourceId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Name)
            .HasColumnName("Name")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar200)
            .HasConversion(name => name.Value, value => SourceName.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(DbConstants.Validations.MaxLengths.L200)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Locator)
            .HasColumnName("Locator")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar2048)
            .HasConversion(locator => locator.Value, value => SourceLocator.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(DbConstants.Validations.MaxLengths.L2048)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Type)
            .HasColumnName("TypeId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(type => type.Id, value => Enumeration.GetAll<SourceType>().Single(type => type.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<SourceStatusType>().Single(status => status.Id == value))
            .IsRequired()
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
            .HasDatabaseName("IX_Sources_WorkspaceId");

        builder.HasIndex(e => new { e.WorkspaceId, e.Locator })
            .IsUnique()
            .HasDatabaseName(DbConstants.Indexes.SourceWorkspaceLocator);

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Sources_StatusId");
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.Sources> builder)
    {
        builder.Property(model => model.SourceId).ValueGeneratedNever();
        builder.Property(model => model.Version).HasDefaultValue(1L).IsConcurrencyToken();
        builder.HasOne<Espada.Db.Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.SourceTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.SourceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Sources_WorkspaceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Sources_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.Locator }).IsUnique().HasDatabaseName(DbConstants.Indexes.SourceWorkspaceLocator);
    }
}