using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncEventConfiguration : IEntityTypeConfiguration<SyncEvents>
    {
        public void Configure(EntityTypeBuilder<SyncEvents> builder)
        {
            builder.Property(entity => entity.EventId).ValueGeneratedNever();
            builder.HasOne<Devices>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.DeviceId, entity.Sequence }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncEventDeviceSequence);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.EntityType, entity.EntityId });
        }
    }
}
