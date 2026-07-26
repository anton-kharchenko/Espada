using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkConfiguration : IEntityTypeConfiguration<Chunk>, IEntityTypeConfiguration<Espada.Db.Models.Chunks>
{
    public void Configure(EntityTypeBuilder<Chunk> builder)
    {
        builder.ToTable(DbConstants.Tables.Chunks, DbConstants.SchemaName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ChunkId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ChunkId.Create(value))
            .IsRequired()
            .ValueGeneratedNever()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.BatchId)
            .HasColumnName("ChunkBatchId")
            .HasColumnType(DbConstants.ColumnTypes.Identifier.Uuid)
            .HasConversion(id => id.Value, value => ChunkBatchId.Create(value))
            .IsRequired()
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

        builder.Property(e => e.Number)
            .HasColumnName("ChunkNumber")
            .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
            .HasConversion(number => number.Value, value => ChunkNumber.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(e => e.Content)
            .HasColumnName("Content")
            .HasColumnType(DbConstants.ColumnTypes.Text.TextType)
            .HasConversion(content => content.Value, value => ChunkContent.Create(value).Value!)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(e => e.SourceSpan, span =>
        {
            span.Property(e => e.Start)
                .HasColumnName("SourceStart")
                .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            span.Property(e => e.Length)
                .HasColumnName("SourceLength")
                .HasColumnType(DbConstants.ColumnTypes.Numeric.Integer)
                .IsRequired()
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            span.Ignore(e => e.EndExclusive);
        });

        builder.Navigation(e => e.SourceSpan)
            .IsRequired(false);

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

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .HasColumnType(DbConstants.ColumnTypes.DateTime.TimestampTz)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<ChunkBatch>()
            .WithMany()
            .HasForeignKey(e => e.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Artifact>()
            .WithMany()
            .HasForeignKey(e => e.ArtifactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ArtifactRevision>()
            .WithMany()
            .HasForeignKey(e => e.ArtifactRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.BatchId)
            .HasDatabaseName("IX_Chunks_ChunkBatchId");

        builder.HasIndex(e => e.WorkspaceId)
            .HasDatabaseName("IX_Chunks_WorkspaceId");

        builder.HasIndex(e => e.ArtifactId)
            .HasDatabaseName("IX_Chunks_ArtifactId");

        builder.HasIndex(e => e.ArtifactRevisionId)
            .HasDatabaseName("IX_Chunks_ArtifactRevisionId");

        builder.HasIndex(e => new { e.BatchId, e.Number })
            .IsUnique()
            .HasDatabaseName(DbConstants.Indexes.ChunkBatchNumber);

        builder.Ignore(e => e.ContentHash);
        builder.Ignore(e => e.SizeInBytes);
        builder.Ignore(e => e.CharacterCount);
        builder.Ignore(e => e.DomainEvents);
    }

    public void Configure(EntityTypeBuilder<Espada.Db.Models.Chunks> builder)
    {
        builder.Property(model => model.ChunkId).ValueGeneratedNever();
        builder.OwnsOne(model => model.SourceSpan, span =>
        {
            span.Property(model => model.Start).HasColumnName("SourceStart").HasColumnType(DbConstants.ColumnTypes.Numeric.Integer);
            span.Property(model => model.Length).HasColumnName("SourceLength").HasColumnType(DbConstants.ColumnTypes.Numeric.Integer);
        });
        builder.Navigation(model => model.SourceSpan).IsRequired(false);
        builder.HasOne<Espada.Db.Models.ChunkBatches>().WithMany().HasForeignKey(model => model.ChunkBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.Artifacts>().WithMany().HasForeignKey(model => model.ArtifactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ArtifactRevisions>().WithMany().HasForeignKey(model => model.ArtifactRevisionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Espada.Db.Models.ChunkingStrategyTypes>().WithMany().HasForeignKey(model => model.StrategyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.ChunkBatchId).HasDatabaseName("IX_Chunks_ChunkBatchId");
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Chunks_WorkspaceId");
        builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_Chunks_ArtifactId");
        builder.HasIndex(model => model.ArtifactRevisionId).HasDatabaseName("IX_Chunks_ArtifactRevisionId");
        builder.HasIndex(model => new { model.ChunkBatchId, model.ChunkNumber }).IsUnique().HasDatabaseName(DbConstants.Indexes.ChunkBatchNumber);
    }
}