using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentApprovalConfiguration : IEntityTypeConfiguration<AgentApprovals>
    {
        public void Configure(EntityTypeBuilder<AgentApprovals> builder)
        {
            builder.Property(entity => entity.AgentApprovalId).ValueGeneratedNever();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<AgentSessions>().WithMany().HasForeignKey(entity => entity.AgentSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<AgentSessionEvents>().WithMany().HasForeignKey(entity => entity.RequestEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentApprovalStatusTypes>().WithMany().HasForeignKey(entity => entity.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => entity.RequestEventId).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentApprovalRequestEvent);
        }
    }
}