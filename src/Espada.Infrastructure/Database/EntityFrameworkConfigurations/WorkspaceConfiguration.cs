using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>,
        IEntityTypeConfiguration<Workspaces>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            ValueConverter<OrganizationId?, Guid?> organizationIdConverter = new(
                id => id == null ? null : id.Value,
                value => value == null ? null : OrganizationId.Create(value.Value));

            builder.ToTable(DbTableConstants.Workspaces, DbConstants.SchemaName);

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("WorkspaceId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
                .IsRequired()
                .ValueGeneratedNever()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.OrganizationId)
                .HasColumnName("OrganizationId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(organizationIdConverter)
                .IsRequired(false)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.Name)
                .HasColumnName("Name")
                .HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasConversion(name => name.Value, value => WorkspaceName.Create(value).Value!)
                .IsRequired()
                .HasMaxLength(DbMaxLengthConstants.L200)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.Type)
                .HasColumnName("TypeId")
                .HasColumnType(DbNumericColumnTypeConstants.Integer)
                .HasConversion(type => type.Id,
                    value => Enumeration.GetAll<WorkspaceType>().Single(type => type.Id == value))
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.Status)
                .HasColumnName("StatusId")
                .HasColumnType(DbNumericColumnTypeConstants.Integer)
                .HasConversion(status => status.Id,
                    value => Enumeration.GetAll<WorkspaceStatusType>().Single(status => status.Id == value))
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.CreatedAtUtc)
                .HasColumnName("CreatedAtUtc")
                .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);


            builder.Property(e => e.ArchivedAtUtc)
                .HasColumnName("ArchivedAtUtc")
                .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired(false)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(e => e.Version)
                .IsRowVersion()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.OrganizationId)
                .HasDatabaseName("IX_Workspaces_OrganizationId");

            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Workspaces_Name");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Workspaces_StatusId");
        }

        public void Configure(EntityTypeBuilder<Workspaces> builder)
        {
            builder.Property(model => model.WorkspaceId).ValueGeneratedNever();
            builder.Property(model => model.Version).IsRowVersion();
            builder.HasOne<Organizations>().WithMany().HasForeignKey(model => model.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<WorkspaceTypes>().WithMany().HasForeignKey(model => model.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<WorkspaceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.OrganizationId).HasDatabaseName("IX_Workspaces_OrganizationId");
            builder.HasIndex(model => model.Name).HasDatabaseName("IX_Workspaces_Name");
            builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Workspaces_StatusId");
        }
    }
}