using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentSessionEventTypeConfiguration : IEntityTypeConfiguration<AgentSessionEventTypes>
    {
        public void Configure(EntityTypeBuilder<AgentSessionEventTypes> builder)
        {
            builder.Property(model => model.AgentSessionEventTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.AgentSessionEventTypeName);
            builder.HasData(Enumeration.GetAll<AgentSessionEventType>().Select(value =>
                new AgentSessionEventTypes { AgentSessionEventTypeId = value.Id, Name = value.Name }));
        }
    }
}