using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class BindingConfiguration : IEntityTypeConfiguration<Bindings>
    {
        public void Configure(EntityTypeBuilder<Bindings> builder)
        {
            builder.ToTable(DbTableConstants.Bindings, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.BindingTaskRequiresProject,
                    "\"TaskId\" IS NULL OR \"ProjectId\" IS NOT NULL"));
            builder.Property(model => model.BindingId).ValueGeneratedNever();
            builder.HasOne<ArtifactRevisions>().WithMany()
                .HasForeignKey(model => new { model.ArtifactRevisionId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.ArtifactRevisionId, model.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Organizations>().WithMany().HasForeignKey(model => model.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Projects>().WithMany().HasForeignKey(model => new { model.ProjectId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.ProjectId, model.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Tasks>().WithMany()
                .HasForeignKey(model => new { model.TaskId, model.ProjectId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.TaskId, model.ProjectId, model.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => new { model.WorkspaceId, model.ArtifactRevisionId })
                .HasDatabaseName("IX_Bindings_WorkspaceId_ArtifactRevisionId");
        }
    }
}