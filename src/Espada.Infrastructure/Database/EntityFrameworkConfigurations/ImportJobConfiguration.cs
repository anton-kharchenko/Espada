using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        ValueConverter<ArtifactId?, Guid?> artifactIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ArtifactId.Create(value.Value));

        ValueConverter<ArtifactRevisionId?, Guid?> revisionIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ArtifactRevisionId.Create(value.Value));

        builder.ToTable(DbConstants.Tables.ImportJobs, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ImportJobId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ImportJobId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.SourceId)
            .HasColumnName("SourceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => SourceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<ImportStatusType>().Single(status => status.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.RequestedAtUtc)
            .HasColumnName("RequestedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.StartedAtUtc)
            .HasColumnName("StartedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CompletedAtUtc)
            .HasColumnName("CompletedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactId)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(artifactIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactRevisionId)
            .HasColumnName("ArtifactRevisionId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(revisionIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(e => e.Failure, failure =>
        {
            failure.Property(e => e.Code)
                .HasColumnName("FailureCode")
                .HasColumnType(DbConstants.ColumnTypes.Text.Varchar200)
                .HasMaxLength(DbConstants.Validations.MaxLengths.L200)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            failure.Property(e => e.Reason)
                .HasColumnName("FailureReason")
                .HasColumnType(DbConstants.ColumnTypes.Text.Varchar4000)
                .HasMaxLength(DbConstants.Validations.MaxLengths.L4000)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(e => e.Failure)
            .IsRequired(false);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_ImportJobs_WorkspaceId");

        builder.HasIndex(e => e.SourceId)
            .HasDatabaseName("IX_ImportJobs_SourceId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_ImportJobs_StatusId");
    }
}