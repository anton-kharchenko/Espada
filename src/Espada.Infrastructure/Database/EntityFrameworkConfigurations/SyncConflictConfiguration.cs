using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncConflictConfiguration : IEntityTypeConfiguration<SyncConflict>
    {
        public void Configure(EntityTypeBuilder<SyncConflict> builder)
        {
            builder.ToTable(DbTableConstants.SyncConflicts, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("SyncConflictId").HasConversion(id => id.Value, value => SyncConflictId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value));
            builder.Property(entity => entity.LocalEventId)
                .HasConversion(id => id.Value, value => SyncEventId.Create(value));
            builder.Property(entity => entity.RemoteEventId)
                .HasConversion(id => id.Value, value => SyncEventId.Create(value));
            builder.Property(entity => entity.EntityType).HasMaxLength(DbMaxLengthConstants.L100);
            builder.Property(entity => entity.DetailsJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.Property(entity => entity.Status).HasColumnName("StatusId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<SyncConflictStatusType>(value));
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<SyncEvent>().WithMany().HasForeignKey(entity => entity.LocalEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<SyncEvent>().WithMany().HasForeignKey(entity => entity.RemoteEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.LocalEventId, entity.RemoteEventId }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncConflictEvents);
        }
    }
}
