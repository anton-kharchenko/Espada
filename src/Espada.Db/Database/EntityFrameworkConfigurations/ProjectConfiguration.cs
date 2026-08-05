using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Projects>
    {
        public void Configure(EntityTypeBuilder<Projects> builder)
        {
            builder.Property(model => model.ProjectId).ValueGeneratedNever();
            builder.Property(model => model.Version).IsRowVersion();
            builder.HasAlternateKey(model => new { model.ProjectId, model.WorkspaceId });
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => new { model.WorkspaceId, model.CanonicalRemoteUri }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ProjectWorkspaceRemote);
        }
    }
}