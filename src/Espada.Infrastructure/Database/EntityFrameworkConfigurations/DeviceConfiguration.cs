using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable(DbTableConstants.Devices, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("DeviceId").HasConversion(id => id.Value, value => DeviceId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.Name).HasMaxLength(DbMaxLengthConstants.L200);
            builder.Property(entity => entity.Version).IsRowVersion();
        }
    }
}