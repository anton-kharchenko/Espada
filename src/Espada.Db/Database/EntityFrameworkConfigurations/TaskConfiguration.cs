using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class TaskConfiguration : IEntityTypeConfiguration<Tasks>
    {
        public void Configure(EntityTypeBuilder<Tasks> builder)
        {
            builder.Property(model => model.TaskId).ValueGeneratedNever();
            builder.Property(model => model.Version).IsRowVersion();
            builder.HasAlternateKey(model => new { model.TaskId, model.ProjectId, model.WorkspaceId });
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Projects>().WithMany().HasForeignKey(model => new { model.ProjectId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.ProjectId, model.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => new { model.WorkspaceId, model.ProjectId, model.Status })
                .HasDatabaseName("IX_Tasks_WorkspaceId_ProjectId_Status");
        }
    }
}