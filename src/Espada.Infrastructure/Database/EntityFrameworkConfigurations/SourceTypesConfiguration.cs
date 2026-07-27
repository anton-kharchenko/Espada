using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class SourceTypesConfiguration : IEntityTypeConfiguration<SourceTypes>
{
    public void Configure(EntityTypeBuilder<SourceTypes> builder)
    {
        builder.Property(model => model.SourceTypeId).ValueGeneratedNever();
        builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbConstants.Indexes.SourceTypeName);
        builder.HasData(Enumeration.GetAll<SourceType>().Select(value => new SourceTypes { SourceTypeId = value.Id, Name = value.Name }));
    }
}