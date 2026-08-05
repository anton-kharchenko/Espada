using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class SyncConflictStatusTypeConfiguration : IEntityTypeConfiguration<SyncConflictStatusTypes>
    {
        public void Configure(EntityTypeBuilder<SyncConflictStatusTypes> builder)
        {
            builder.Property(model => model.SyncConflictStatusTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.SyncConflictStatusTypeName);
            builder.HasData(Enumeration.GetAll<SyncConflictStatusType>().Select(value =>
                new SyncConflictStatusTypes { SyncConflictStatusTypeId = value.Id, Name = value.Name }));
        }
    }
}