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
    internal sealed class AgentApprovalConfiguration : IEntityTypeConfiguration<AgentApproval>
    {
        public void Configure(EntityTypeBuilder<AgentApproval> builder)
        {
            builder.ToTable(DbTableConstants.AgentApprovals, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("AgentApprovalId").HasConversion(id => id.Value, value => AgentApprovalId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.AgentSessionId)
                .HasConversion(id => id.Value, value => AgentSessionId.Create(value));
            builder.Property(entity => entity.RequestEventId)
                .HasConversion(id => id.Value, value => AgentSessionEventId.Create(value));
            builder.Property(entity => entity.ToolName).HasMaxLength(DbMaxLengthConstants.L200);
            builder.Property(entity => entity.ArgumentsJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.Property(entity => entity.Status).HasColumnName("StatusId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<AgentApprovalStatusType>(value));
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<AgentSession>().WithMany().HasForeignKey(entity => entity.AgentSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<AgentSessionEvent>().WithMany().HasForeignKey(entity => entity.RequestEventId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => entity.RequestEventId).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentApprovalRequestEvent);
        }
    }
}
