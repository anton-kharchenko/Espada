using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMemberships>
    {
        public void Configure(EntityTypeBuilder<OrganizationMemberships> builder)
        {
            builder.Property(model => model.OrganizationMembershipId).ValueGeneratedNever();
            builder.HasOne<Organizations>().WithMany().HasForeignKey(model => model.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => new { model.OrganizationId, model.Issuer, model.Subject }).IsUnique()
                .HasDatabaseName(DbIndexConstants.OrganizationMembershipIdentity);
        }
    }
}