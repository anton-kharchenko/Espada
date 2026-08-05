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
    internal sealed class AgentSessionEventConfiguration : IEntityTypeConfiguration<AgentSessionEvent>
    {
        public void Configure(EntityTypeBuilder<AgentSessionEvent> builder)
        {
            builder.ToTable(DbTableConstants.AgentSessionEvents, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("AgentSessionEventId")
                .HasConversion(id => id.Value, value => AgentSessionEventId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.AgentSessionId)
                .HasConversion(id => id.Value, value => AgentSessionId.Create(value));
            builder.Property(entity => entity.Type).HasColumnName("TypeId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<AgentSessionEventType>(value));
            builder.Property(entity => entity.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.HasOne<AgentSession>().WithMany().HasForeignKey(entity => entity.AgentSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(entity => new { entity.AgentSessionId, entity.Sequence }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentSessionEventSequence);
        }
    }
}