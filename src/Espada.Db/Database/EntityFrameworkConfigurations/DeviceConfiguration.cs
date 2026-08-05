using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Devices>
    {
        public void Configure(EntityTypeBuilder<Devices> builder)
        {
            builder.Property(entity => entity.DeviceId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
        }
    }
}
