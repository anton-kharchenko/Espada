using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentApprovalStatusTypeConfiguration : IEntityTypeConfiguration<AgentApprovalStatusTypes>
    {
        public void Configure(EntityTypeBuilder<AgentApprovalStatusTypes> builder)
        {
            builder.Property(model => model.AgentApprovalStatusTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.AgentApprovalStatusTypeName);
            builder.HasData(Enumeration.GetAll<AgentApprovalStatusType>().Select(value =>
                new AgentApprovalStatusTypes { AgentApprovalStatusTypeId = value.Id, Name = value.Name }));
        }
    }
}
