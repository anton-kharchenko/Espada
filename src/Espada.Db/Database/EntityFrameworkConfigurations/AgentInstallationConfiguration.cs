using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentInstallationConfiguration : IEntityTypeConfiguration<AgentInstallations>
    {
        public void Configure(EntityTypeBuilder<AgentInstallations> builder)
        {
            builder.Property(entity => entity.AgentInstallationId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Devices>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentVendorTypes>().WithMany().HasForeignKey(entity => entity.VendorTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.DeviceId, entity.VendorTypeId, entity.ExecutablePath }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentInstallationDeviceVendorPath);
        }
    }
}
