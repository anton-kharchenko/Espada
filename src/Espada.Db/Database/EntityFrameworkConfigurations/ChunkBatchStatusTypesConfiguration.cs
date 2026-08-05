using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class ChunkBatchStatusTypesConfiguration : IEntityTypeConfiguration<ChunkBatchStatusTypes>
    {
        public void Configure(EntityTypeBuilder<ChunkBatchStatusTypes> builder)
        {
            builder.Property(model => model.ChunkBatchStatusTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.ChunkBatchStatusTypeName);
            builder.HasData(
                Enumeration.GetAll<ChunkBatchStatusType>()
                    .Select(value =>
                        new ChunkBatchStatusTypes { ChunkBatchStatusTypeId = value.Id, Name = value.Name }));
        }
    }
}