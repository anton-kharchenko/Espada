using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>, IEntityTypeConfiguration<Espada.Db.Models.Sources>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable(
            DbTableConstants.Sources,
            DbConstants.SchemaName,
            table => table.HasCheckConstraint(DbConstraintConstants.SourcePriorityRange, CheckConstraintSql.ContextPriority(nameof(Source.Priority))));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("SourceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => SourceId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Name)
            .HasColumnName("Name")
            .HasColumnType(DbTextColumnTypeConstants.Varchar200)
            .HasConversion(name => name.Value, value => SourceName.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(DbMaxLengthConstants.L200)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Locator)
            .HasColumnName("Locator")
            .HasColumnType(DbTextColumnTypeConstants.Varchar2048)
            .HasConversion(locator => locator.Value, value => SourceLocator.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(DbMaxLengthConstants.L2048)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        ValueComparer<SourceDefinition?> definitionComparer = new(
            (left, right) => left == right || left != null && right != null
                && SourceDefinitionSerializer.Serialize(left) == SourceDefinitionSerializer.Serialize(right),
            definition => definition == null ? 0 : SourceDefinitionSerializer.Serialize(definition).GetHashCode(StringComparison.Ordinal),
            definition => definition == null ? null : SourceDefinitionSerializer.Deserialize(SourceDefinitionSerializer.Serialize(definition)));

        builder.Property<SourceDefinition?>("_definition")
            .HasColumnName("DefinitionJson")
            .HasColumnType(DbJsonColumnTypeConstants.Jsonb)
            .HasConversion(
                definition => definition == null ? null : SourceDefinitionSerializer.Serialize(definition),
                json => json == null ? null : SourceDefinitionSerializer.Deserialize(json))
            .IsRequired(false)
            .Metadata.SetValueComparer(definitionComparer);

        builder.Property(e => e.Type)
            .HasColumnName("TypeId")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(type => type.Id, value => Enumeration.GetAll<SourceType>().Single(type => type.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<SourceStatusType>().Single(status => status.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Priority)
            .HasColumnName("Priority")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(priority => priority.Value, value => ContextPriority.Create(value).Value!)
            .HasDefaultValue(ContextPriority.Neutral)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("UpdatedAtUtc")
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

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(e => e.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_Sources_WorkspaceId");

        builder.HasIndex(e => new { e.WorkspaceId, e.Locator })
            .IsUnique()
            .HasDatabaseName(DbIndexConstants.SourceWorkspaceLocator);

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Sources_StatusId");
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.Sources> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            DbConstraintConstants.SourcePriorityRange,
            CheckConstraintSql.ContextPriority(nameof(Db.Models.Sources.Priority))));
        builder.Property(model => model.SourceId).ValueGeneratedNever();
        builder.Property(model => model.Priority).HasDefaultValue(ContextPriority.Neutral.Value);
        builder.Property(model => model.Version).IsRowVersion();
        builder.Property(model => model.DefinitionJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb).IsRequired(false);
        builder.HasOne<Espada.Db.Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.SourceTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.SourceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Sources_WorkspaceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Sources_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.Locator }).IsUnique().HasDatabaseName(DbIndexConstants.SourceWorkspaceLocator);
    }
}