using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspaces>
    {
        public void Configure(EntityTypeBuilder<Workspaces> builder)
        {
            builder.Property(model => model.WorkspaceId).ValueGeneratedNever();
            builder.Property(model => model.Version).IsRowVersion();
            builder.HasOne<Organizations>().WithMany().HasForeignKey(model => model.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<WorkspaceTypes>().WithMany().HasForeignKey(model => model.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<WorkspaceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.OrganizationId).HasDatabaseName("IX_Workspaces_OrganizationId");
            builder.HasIndex(model => model.Name).HasDatabaseName("IX_Workspaces_Name");
            builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Workspaces_StatusId");
        }
    }
}