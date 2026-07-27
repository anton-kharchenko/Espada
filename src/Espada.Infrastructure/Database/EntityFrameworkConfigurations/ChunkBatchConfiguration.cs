using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkBatchConfiguration : IEntityTypeConfiguration<ChunkBatch>, IEntityTypeConfiguration<Espada.Db.Models.ChunkBatches>
{
    public void Configure(EntityTypeBuilder<ChunkBatch> builder)
    {
        builder.ToTable(DbTableConstants.ChunkBatches, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ChunkBatchId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ChunkBatchId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactId)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ArtifactId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactRevisionId)
            .HasColumnName("ArtifactRevisionId")
            .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
            .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Strategy)
            .HasColumnName("StrategyId")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(strategy => strategy.Id, value => Enumeration.GetAll<ChunkingStrategyType>().Single(strategy => strategy.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.StrategyVersion)
            .HasColumnName("StrategyVersion")
            .HasColumnType(DbTextColumnTypeConstants.Varchar64)
            .HasConversion(version => version.Value, value => ChunkingVersion.Create(value).Value!)
            .HasMaxLength(DbMaxLengthConstants.L64)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<ChunkBatchStatusType>().Single(status => status.Id == value))
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

        builder.Property(e => e.ChunkCount)
            .HasColumnName("ChunkCount")
            .HasColumnType(DbNumericColumnTypeConstants.Integer)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.FailureReason)
            .HasColumnName("FailureReason")
            .HasColumnType(DbTextColumnTypeConstants.TextType)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Version)
            .IsRowVersion()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<ArtifactRevision>()
            .WithMany()
            .HasForeignKey(e => e.ArtifactRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_ChunkBatches_WorkspaceId");

        builder.HasIndex(e => e.ArtifactId)
            .HasDatabaseName("IX_ChunkBatches_ArtifactId");

        builder.HasIndex(e => e.ArtifactRevisionId)
            .HasDatabaseName("IX_ChunkBatches_ArtifactRevisionId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_ChunkBatches_StatusId");

        builder.HasIndex(e => new { e.ArtifactRevisionId, e.Strategy, e.StrategyVersion })
            .HasDatabaseName("IX_ChunkBatches_Revision_Strategy_Version");

        builder.Ignore(e => e.DomainEvents);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.ChunkBatches> builder)
    {
        builder.Property(model => model.ChunkBatchId).ValueGeneratedNever();
        builder.Property(model => model.Version).IsRowVersion();
        builder.HasOne<Espada.Db.Models.ArtifactRevisions>().WithMany().HasForeignKey(model => model.ArtifactRevisionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ChunkingStrategyTypes>().WithMany().HasForeignKey(model => model.StrategyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ChunkBatchStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ChunkBatches_WorkspaceId");
        builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ChunkBatches_ArtifactId");
        builder.HasIndex(model => model.ArtifactRevisionId).HasDatabaseName("IX_ChunkBatches_ArtifactRevisionId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_ChunkBatches_StatusId");
        builder
            .HasIndex(
                model => new
                {
                    model.ArtifactRevisionId,
                    model.StrategyId,
                    model.StrategyVersion
                })
            .HasDatabaseName(
                "IX_ChunkBatches_Revision_Strategy_Version");
    }
}