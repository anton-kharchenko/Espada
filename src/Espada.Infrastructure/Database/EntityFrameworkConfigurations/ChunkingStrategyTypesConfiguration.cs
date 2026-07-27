using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class ChunkingStrategyTypesConfiguration : IEntityTypeConfiguration<ChunkingStrategyTypes>
{
    public void Configure(EntityTypeBuilder<ChunkingStrategyTypes> builder)
    {
        builder.Property(model => model.ChunkingStrategyTypeId).ValueGeneratedNever();
        builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbConstants.Indexes.ChunkingStrategyTypeName);
        builder.HasData(Enumeration.GetAll<ChunkingStrategyType>().Select(value => new ChunkingStrategyTypes { ChunkingStrategyTypeId = value.Id, Name = value.Name }));
    }
}