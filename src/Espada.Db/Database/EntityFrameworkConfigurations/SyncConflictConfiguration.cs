using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncConflictConfiguration : IEntityTypeConfiguration<SyncConflicts>
    {
        public void Configure(EntityTypeBuilder<SyncConflicts> builder)
        {
            builder.Property(entity => entity.SyncConflictId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspaces>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<SyncEvents>().WithMany().HasForeignKey(entity => entity.LocalEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<SyncEvents>().WithMany().HasForeignKey(entity => entity.RemoteEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<SyncConflictStatusTypes>().WithMany().HasForeignKey(entity => entity.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.LocalEventId, entity.RemoteEventId }).IsUnique()
                .HasDatabaseName(DbIndexConstants.SyncConflictEvents);
        }
    }
}