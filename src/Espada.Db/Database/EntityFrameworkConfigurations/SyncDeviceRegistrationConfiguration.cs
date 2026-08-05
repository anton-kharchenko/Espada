using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncDeviceRegistrationConfiguration : IEntityTypeConfiguration<SyncDeviceRegistrations>
    {
        public void Configure(EntityTypeBuilder<SyncDeviceRegistrations> builder)
        {
            builder.HasKey(entity => entity.DeviceId);
            builder.Property(entity => entity.DeviceId).ValueGeneratedNever();
            builder.Property(entity => entity.Issuer).HasMaxLength(DbMaxLengthConstants.L500);
            builder.Property(entity => entity.Subject).HasMaxLength(DbMaxLengthConstants.L500);
            builder.HasOne<Devices>().WithOne().HasForeignKey<SyncDeviceRegistrations>(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(entity => new { entity.Issuer, entity.Subject });
        }
    }
}