using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncDeviceRegistrationConfiguration : IEntityTypeConfiguration<SyncDeviceRegistrations>
    {
        public void Configure(EntityTypeBuilder<SyncDeviceRegistrations> builder)
        {
            builder.ToTable(DbTableConstants.SyncDeviceRegistrations, DbConstants.SchemaName);
            builder.HasKey(entity => entity.DeviceId);
            builder.Property(entity => entity.DeviceId).ValueGeneratedNever();
            builder.Property(entity => entity.Issuer).HasMaxLength(DbMaxLengthConstants.L500);
            builder.Property(entity => entity.Subject).HasMaxLength(DbMaxLengthConstants.L500);
            builder.HasIndex(entity => new { entity.Issuer, entity.Subject });
        }
    }
}