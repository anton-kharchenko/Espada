using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentSessionConfiguration : IEntityTypeConfiguration<AgentSessions>
    {
        public void Configure(EntityTypeBuilder<AgentSessions> builder)
        {
            builder.Property(entity => entity.AgentSessionId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Projects>().WithMany()
                .HasForeignKey(entity => new { entity.ProjectId, entity.WorkspaceId })
                .HasPrincipalKey(entity => new { entity.ProjectId, entity.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentProfiles>().WithMany().HasForeignKey(entity => entity.AgentProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Devices>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentSessionStatusTypes>().WithMany().HasForeignKey(entity => entity.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.CreatedAtUtc });
        }
    }
}