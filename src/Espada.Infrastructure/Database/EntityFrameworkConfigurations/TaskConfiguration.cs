using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTask = Espada.Domain.Aggregates.ProjectTask;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class TaskConfiguration : IEntityTypeConfiguration<ProjectTask>
    {
        public void Configure(EntityTypeBuilder<ProjectTask> builder)
        {
            builder.ToTable(DbTableConstants.Tasks, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("TaskId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => TaskId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.WorkspaceId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value)).IsRequired();
            builder.Property(entity => entity.ProjectId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ProjectId.Create(value)).IsRequired();
            builder.Property(entity => entity.Title).HasColumnType(DbTextColumnTypeConstants.Varchar500)
                .HasMaxLength(DbMaxLengthConstants.L500).IsRequired();
            builder.Property(entity => entity.Status).HasColumnType(DbTextColumnTypeConstants.Varchar32)
                .HasMaxLength(DbMaxLengthConstants.L32).HasConversion(status => status.Name,
                    value => Enumeration.GetAll<TaskStatusType>().Single(status => status.Name == value)).IsRequired();
            builder.Property(entity => entity.CreatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.UpdatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.CompletedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz);
            builder.Property(entity => entity.ArchivedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz);
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasAlternateKey(entity => new { entity.Id, entity.ProjectId, entity.WorkspaceId });
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Project>().WithMany().HasForeignKey(entity => new { entity.ProjectId, entity.WorkspaceId })
                .HasPrincipalKey(project => new { project.Id, project.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.ProjectId, entity.Status })
                .HasDatabaseName("IX_Tasks_WorkspaceId_ProjectId_Status");
        }
    }
}