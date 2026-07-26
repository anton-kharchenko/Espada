using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>, IEntityTypeConfiguration<Espada.Db.Models.Workspaces>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable(DbConstants.Tables.Workspaces, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Name)
            .HasColumnName("Name")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar200)
            .HasConversion(name => name.Value, value => WorkspaceName.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(DbConstants.Validations.MaxLengths.L200)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Type)
            .HasColumnName("TypeId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(type => type.Id, value => Enumeration.GetAll<WorkspaceType>().Single(type => type.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<WorkspaceStatusType>().Single(status => status.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
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

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Workspaces_Name");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Workspaces_StatusId");
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.Workspaces> builder)
    {
        builder.Property(model => model.WorkspaceId).ValueGeneratedNever();
        builder.Property(model => model.Version).HasDefaultValue(1L).IsConcurrencyToken();
        builder.HasOne<Espada.Db.Models.WorkspaceTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.WorkspaceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.Name).HasDatabaseName("IX_Workspaces_Name");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Workspaces_StatusId");
    }
}