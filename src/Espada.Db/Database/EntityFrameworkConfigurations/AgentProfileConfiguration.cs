using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentProfileConfiguration : IEntityTypeConfiguration<AgentProfiles>
    {
        public void Configure(EntityTypeBuilder<AgentProfiles> builder)
        {
            builder.Property(entity => entity.AgentProfileId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentVendorTypes>().WithMany().HasForeignKey(entity => entity.VendorTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.VendorTypeId, entity.Name }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentProfileWorkspaceVendorName);
        }
    }
}