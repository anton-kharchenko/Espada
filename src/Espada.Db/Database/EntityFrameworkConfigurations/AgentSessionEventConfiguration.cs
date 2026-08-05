using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentSessionEventConfiguration : IEntityTypeConfiguration<AgentSessionEvents>
    {
        public void Configure(EntityTypeBuilder<AgentSessionEvents> builder)
        {
            builder.Property(entity => entity.AgentSessionEventId).ValueGeneratedNever();
            builder.HasOne<AgentSessions>().WithMany().HasForeignKey(entity => entity.AgentSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<AgentSessionEventTypes>().WithMany().HasForeignKey(entity => entity.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.AgentSessionId, entity.Sequence }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentSessionEventSequence);
        }
    }
}