using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentSessionStatusTypeConfiguration : IEntityTypeConfiguration<AgentSessionStatusTypes>
    {
        public void Configure(EntityTypeBuilder<AgentSessionStatusTypes> builder)
        {
            builder.Property(model => model.AgentSessionStatusTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.AgentSessionStatusTypeName);
            builder.HasData(Enumeration.GetAll<AgentSessionStatusType>().Select(value =>
                new AgentSessionStatusTypes { AgentSessionStatusTypeId = value.Id, Name = value.Name }));
        }
    }
}
