using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkBatchConfiguration : IEntityTypeConfiguration<ChunkBatch>
{
    public void Configure(EntityTypeBuilder<ChunkBatch> builder)
    {
        builder.ToTable(DbConstants.Tables.ChunkBatches, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ChunkBatchId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ChunkBatchId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.WorkspaceId)
            .HasColumnName("WorkspaceId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactId)
            .HasColumnName("ArtifactId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ArtifactId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.ArtifactRevisionId)
            .HasColumnName("ArtifactRevisionId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Strategy)
            .HasColumnName("StrategyId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(strategy => strategy.Id, value => Enumeration.GetAll<ChunkingStrategyType>().Single(strategy => strategy.Id == value))
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.StrategyVersion)
            .HasColumnName("StrategyVersion")
            .HasColumnType(DbConstants.ColumnTypes.Text.Varchar64)
            .HasConversion(version => version.Value, value => ChunkingVersion.Create(value).Value!)
            .HasMaxLength(DbConstants.Validations.MaxLengths.L64)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Status)
            .HasColumnName("StatusId")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(status => status.Id, value => Enumeration.GetAll<ChunkBatchStatusType>().Single(status => status.Id == value))
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

        builder.Property(e => e.ChunkCount)
            .HasColumnName("ChunkCount")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.FailureReason)
            .HasColumnName("FailureReason")
            .HasColumnType(DbConstants.ColumnTypes.Text.TextType)
            .IsRequired(false)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Version)
            .HasColumnName("Version")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.BigInt)
            .HasDefaultValue(1L)
            .IsConcurrencyToken()
            .IsRequired()
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
}