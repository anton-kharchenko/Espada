using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncEventConfiguration : IEntityTypeConfiguration<SyncEvent>
    {
        public void Configure(EntityTypeBuilder<SyncEvent> builder)
        {
            builder.ToTable(DbTableConstants.SyncEvents, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property<long>("ServerSequence").UseIdentityAlwaysColumn();
            builder.Property(entity => entity.Id).HasColumnName("EventId")
                .HasConversion(id => id.Value, value => SyncEventId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.DeviceId)
                .HasConversion(id => id.Value, value => DeviceId.Create(value));
            builder.Property(entity => entity.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value));
            builder.Property(entity => entity.EntityType).HasMaxLength(DbMaxLengthConstants.L100);
            builder.Property(entity => entity.Operation).HasMaxLength(DbMaxLengthConstants.L32);
            builder.Property(entity => entity.PayloadType).HasMaxLength(DbMaxLengthConstants.L100);
            builder.Property(entity => entity.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.Property(entity => entity.PayloadHash).HasMaxLength(DbMaxLengthConstants.L100);
            builder.HasOne<Device>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex("ServerSequence").IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncEventServerSequence);
            builder.HasIndex(entity => new { entity.DeviceId, entity.Sequence }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncEventDeviceSequence);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.EntityType, entity.EntityId });
        }
    }
}