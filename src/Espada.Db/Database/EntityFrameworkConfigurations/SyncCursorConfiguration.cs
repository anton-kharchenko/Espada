using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncCursorConfiguration : IEntityTypeConfiguration<SyncCursors>
    {
        public void Configure(EntityTypeBuilder<SyncCursors> builder)
        {
            builder.Property(entity => entity.SyncCursorId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Devices>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.DeviceId, entity.WorkspaceId }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncCursorDeviceWorkspace);
        }
    }
}