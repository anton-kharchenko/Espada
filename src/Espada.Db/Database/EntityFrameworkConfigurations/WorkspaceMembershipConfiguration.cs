using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class WorkspaceMembershipConfiguration : IEntityTypeConfiguration<WorkspaceMemberships>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMemberships> builder)
        {
            builder.HasKey(membership => membership.WorkspaceMembershipId);
            builder.Property(membership => membership.WorkspaceMembershipId).ValueGeneratedNever();
            builder.Property(membership => membership.Issuer).HasMaxLength(500).IsRequired();
            builder.Property(membership => membership.Subject).HasMaxLength(200).IsRequired();
            builder.HasIndex(membership => new { membership.WorkspaceId, membership.Issuer, membership.Subject })
                .IsUnique();
            builder.HasOne<Workspaces>()
                .WithMany()
                .HasForeignKey(membership => membership.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}