using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectTask = Espada.Domain.Aggregates.ProjectTask;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class BindingConfiguration : IEntityTypeConfiguration<Binding>
    {
        public void Configure(EntityTypeBuilder<Binding> builder)
        {
            ValueConverter<OrganizationId?, Guid?> organizationIdConverter = new(id => id == null ? null : id.Value,
                value => value == null ? null : OrganizationId.Create(value.Value));
            ValueConverter<ProjectId?, Guid?> projectIdConverter = new(id => id == null ? null : id.Value,
                value => value == null ? null : ProjectId.Create(value.Value));
            ValueConverter<TaskId?, Guid?> taskIdConverter = new(id => id == null ? null : id.Value,
                value => value == null ? null : TaskId.Create(value.Value));
            builder.ToTable(DbTableConstants.Bindings, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.BindingTaskRequiresProject,
                    "\"TaskId\" IS NULL OR \"ProjectId\" IS NOT NULL"));
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("BindingId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => BindingId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.ArtifactRevisionId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ArtifactRevisionId.Create(value)).IsRequired();
            builder.Property(entity => entity.OrganizationId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(organizationIdConverter);
            builder.Property(entity => entity.WorkspaceId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value)).IsRequired();
            builder.Property(entity => entity.ProjectId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(projectIdConverter);
            builder.Property(entity => entity.RepositoryCanonicalUri)
                .HasColumnType(DbTextColumnTypeConstants.Varchar2048).HasMaxLength(DbMaxLengthConstants.L2048);
            builder.Property(entity => entity.RepositoryRelativePathPrefix)
                .HasColumnType(DbTextColumnTypeConstants.Varchar2000).HasMaxLength(DbMaxLengthConstants.L2000);
            builder.Property(entity => entity.Branch).HasColumnType(DbTextColumnTypeConstants.Varchar500)
                .HasMaxLength(DbMaxLengthConstants.L500);
            builder.Property(entity => entity.TaskId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(taskIdConverter);
            builder.Property(entity => entity.Agent).HasColumnType(DbTextColumnTypeConstants.Varchar100)
                .HasMaxLength(DbMaxLengthConstants.L100);
            builder.Property(entity => entity.CreatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.HasOne<ArtifactRevision>().WithMany()
                .HasForeignKey(entity => new { entity.ArtifactRevisionId, entity.WorkspaceId })
                .HasPrincipalKey(revision => new { revision.Id, revision.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Organization>().WithMany().HasForeignKey(entity => entity.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Project>().WithMany().HasForeignKey(entity => new { entity.ProjectId, entity.WorkspaceId })
                .HasPrincipalKey(project => new { project.Id, project.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ProjectTask>().WithMany()
                .HasForeignKey(entity => new { entity.TaskId, entity.ProjectId, entity.WorkspaceId })
                .HasPrincipalKey(task => new { task.Id, task.ProjectId, task.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.ArtifactRevisionId })
                .HasDatabaseName("IX_Bindings_WorkspaceId_ArtifactRevisionId");
        }
    }
}