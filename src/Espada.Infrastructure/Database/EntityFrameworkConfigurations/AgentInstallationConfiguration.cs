using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentInstallationConfiguration : IEntityTypeConfiguration<AgentInstallation>
    {
        public void Configure(EntityTypeBuilder<AgentInstallation> builder)
        {
            builder.ToTable(DbTableConstants.AgentInstallations, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("AgentInstallationId")
                .HasConversion(id => id.Value, value => AgentInstallationId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.DeviceId)
                .HasConversion(id => id.Value, value => DeviceId.Create(value));
            builder.Property(entity => entity.Vendor).HasColumnName("VendorTypeId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<AgentVendorType>(value));
            builder.Property(entity => entity.ExecutablePath).HasMaxLength(DbMaxLengthConstants.L2048);
            builder.Property(entity => entity.DetectedVersion).HasMaxLength(DbMaxLengthConstants.L100);
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Device>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.DeviceId, entity.Vendor, entity.ExecutablePath }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentInstallationDeviceVendorPath);
        }
    }
}