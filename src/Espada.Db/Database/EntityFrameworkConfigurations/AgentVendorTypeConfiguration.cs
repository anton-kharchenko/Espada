using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentVendorTypeConfiguration : IEntityTypeConfiguration<AgentVendorTypes>
    {
        public void Configure(EntityTypeBuilder<AgentVendorTypes> builder)
        {
            builder.Property(model => model.AgentVendorTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.AgentVendorTypeName);
            builder.HasData(Enumeration.GetAll<AgentVendorType>().Select(value =>
                new AgentVendorTypes { AgentVendorTypeId = value.Id, Name = value.Name }));
        }
    }
}