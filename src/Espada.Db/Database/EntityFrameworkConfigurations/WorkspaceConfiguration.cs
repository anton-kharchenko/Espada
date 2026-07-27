using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Models.Workspaces>
{
    public void Configure(EntityTypeBuilder<Models.Workspaces> builder)
    {
        builder.Property(model => model.WorkspaceId).ValueGeneratedNever();
        builder.Property(model => model.Version).IsRowVersion();
        builder.HasOne<Models.WorkspaceTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.WorkspaceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.Name).HasDatabaseName("IX_Workspaces_Name");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Workspaces_StatusId");
    }
}