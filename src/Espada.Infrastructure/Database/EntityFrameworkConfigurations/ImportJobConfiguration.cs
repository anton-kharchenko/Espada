using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>, IEntityTypeConfiguration<Espada.Db.Models.ImportJobs>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        ValueConverter<ArtifactId?, Guid?> artifactIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ArtifactId.Create(value.Value));

        ValueConverter<ArtifactRevisionId?, Guid?> revisionIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ArtifactRevisionId.Create(value.Value));
        ValueConverter<ChunkBatchId?, Guid?> chunkBatchIdConverter = new(
            id => id == null ? null : id.Value,
            value => value == null ? null : ChunkBatchId.Create(value.Value));

        builder.ToTable(DbTableConstants.ImportJobs, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ImportJobId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ImportJobId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.SourceId)
            .HasColumnName("SourceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => SourceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<ImportStatusType>().Single(status => status.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Stage)
            .HasColumnName("Stage")
            .HasConversion(
                stage => stage.Id,
                value => Enumeration.GetAll<ImportPipelineStageType>()
                    .Single(stage => stage.Id == value))
            .HasDefaultValue(ImportPipelineStageType.Start)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.IdempotencyKey)
            .HasColumnName("IdempotencyKey")
            .HasMaxLength(200)
            .HasDefaultValueSql("gen_random_uuid()::text")
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.RequestFingerprint)
            .HasColumnName("RequestFingerprint")
            .HasMaxLength(64)
            .HasDefaultValueSql("gen_random_uuid()::text")
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.OptionsJson)
            .HasColumnName("OptionsJson")
            .HasColumnType(DbJsonColumnTypeConstants.Jsonb)
            .HasDefaultValue("{}")
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.RequestedAtUtc)
            .HasColumnName("RequestedAtUtc")
            .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.StartedAtUtc)
            .HasColumnName("StartedAtUtc")
            .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.CompletedAtUtc)
            .HasColumnName("CompletedAtUtc")
            .HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactId)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(artifactIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactRevisionId)
            .HasColumnName("ArtifactRevisionId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(revisionIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ChunkBatchId)
            .HasColumnName("ChunkBatchId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(chunkBatchIdConverter)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.RawBlobHash)
            .HasColumnName("RawBlobHash")
            .HasMaxLength(200)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ParsedBlobHash)
            .HasColumnName("ParsedBlobHash")
            .HasMaxLength(200)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(e => e.Failure, failure =>
        {
            failure.Property(e => e.Code)
                .HasColumnName("FailureCode")
                .HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            failure.Property(e => e.Reason)
                .HasColumnName("FailureReason")
                .HasColumnType(DbTextColumnTypeConstants.Varchar4000)
                .HasMaxLength(DbMaxLengthConstants.L4000)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(e => e.Failure)
            .IsRequired(false);

        builder.Property(e => e.Version)
            .IsRowVersion()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Source>()
            .WithMany()
            .HasForeignKey(e => e.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(e => e.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_ImportJobs_WorkspaceId");

        builder.HasIndex(e => e.SourceId)
            .HasDatabaseName("IX_ImportJobs_SourceId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_ImportJobs_StatusId");

        builder.HasIndex(e => new { e.WorkspaceId, e.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("UX_ImportJobs_WorkspaceId_IdempotencyKey");
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.ImportJobs> builder)
    {
        builder.Property(model => model.ImportJobId).ValueGeneratedNever();
        builder.Property(model => model.Version).IsRowVersion();
        builder.Property(model => model.Stage)
            .HasDefaultValue(ImportPipelineStageType.Start.Id);
        builder.Property(model => model.IdempotencyKey).HasDefaultValueSql("gen_random_uuid()::text");
        builder.Property(model => model.RequestFingerprint).HasDefaultValueSql("gen_random_uuid()::text");
        builder.Property(model => model.OptionsJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb).HasDefaultValue("{}");
        builder.OwnsOne(model => model.Failure, failure =>
        {
            failure
                .Property(model => model.Code)
                .HasColumnName("FailureCode")
                .HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200);
            failure
                .Property(model => model.Reason)
                .HasColumnName("FailureReason")
                .HasColumnType(DbTextColumnTypeConstants.Varchar4000)
                .HasMaxLength(DbMaxLengthConstants.L4000);
        });
        builder.Navigation(model => model.Failure).IsRequired(false);
        builder.HasOne<Espada.Db.Models.Sources>().WithMany().HasForeignKey(model => model.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ImportStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ImportJobs_WorkspaceId");
        builder.HasIndex(model => model.SourceId).HasDatabaseName("IX_ImportJobs_SourceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_ImportJobs_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.IdempotencyKey }).IsUnique().HasDatabaseName("UX_ImportJobs_WorkspaceId_IdempotencyKey");
    }
}