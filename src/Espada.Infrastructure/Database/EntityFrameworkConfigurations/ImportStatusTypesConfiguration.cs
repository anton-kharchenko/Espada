using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ImportStatusTypesConfiguration : IEntityTypeConfiguration<ImportStatusTypes>
{
    public void Configure(EntityTypeBuilder<ImportStatusTypes> builder)
    {
        builder.Property(model => model.ImportStatusTypeId).ValueGeneratedNever();
        builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.ImportStatusTypeName);
        builder.HasData(Enumeration.GetAll<ImportStatusType>().Select(value => new ImportStatusTypes { ImportStatusTypeId = value.Id, Name = value.Name }));
    }
}