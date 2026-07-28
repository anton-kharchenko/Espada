using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SourceStatusTypesConfiguration : IEntityTypeConfiguration<SourceStatusTypes>
    {
        public void Configure(EntityTypeBuilder<SourceStatusTypes> builder)
        {
            builder.Property(model => model.SourceStatusTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.SourceStatusTypeName);
            builder.HasData(Enumeration.GetAll<SourceStatusType>().Select(value =>
                new SourceStatusTypes { SourceStatusTypeId = value.Id, Name = value.Name }));
        }
    }
}