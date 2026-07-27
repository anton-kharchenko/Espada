using Espada.Db.Constants;
using Espada.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ImportJobConfiguration : IEntityTypeConfiguration<Models.ImportJobs>
{
    public void Configure(EntityTypeBuilder<Models.ImportJobs> builder)
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
        builder.HasOne<Models.Sources>().WithMany().HasForeignKey(model => model.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.ImportStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_ImportJobs_WorkspaceId");
        builder.HasIndex(model => model.SourceId).HasDatabaseName("IX_ImportJobs_SourceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_ImportJobs_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.IdempotencyKey }).IsUnique().HasDatabaseName("UX_ImportJobs_WorkspaceId_IdempotencyKey");
    }
}