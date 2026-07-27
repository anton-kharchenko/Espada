using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ArtifactTypesConfiguration : IEntityTypeConfiguration<ArtifactTypes>
{
    public void Configure(EntityTypeBuilder<ArtifactTypes> builder)
    {
        builder.Property(model => model.ArtifactTypeId).ValueGeneratedNever();
        builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.ArtifactTypeName);
        builder.HasData(Enumeration.GetAll<ArtifactType>().Select(value => new ArtifactTypes { ArtifactTypeId = value.Id, Name = value.Name }));
    }
}