using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncCursorConfiguration : IEntityTypeConfiguration<SyncCursor>
    {
        public void Configure(EntityTypeBuilder<SyncCursor> builder)
        {
            builder.ToTable(DbTableConstants.SyncCursors, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("SyncCursorId").HasConversion(id => id.Value, value => SyncCursorId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.DeviceId)
                .HasConversion(id => id.Value, value => DeviceId.Create(value));
            builder.Property(entity => entity.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value));
            builder.Property(entity => entity.ServerCursor).HasMaxLength(DbMaxLengthConstants.L500);
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Device>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.DeviceId, entity.WorkspaceId }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncCursorDeviceWorkspace);
        }
    }
}