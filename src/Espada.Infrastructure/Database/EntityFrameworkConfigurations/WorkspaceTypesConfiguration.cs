using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class WorkspaceTypesConfiguration : IEntityTypeConfiguration<WorkspaceTypes>
    {
        public void Configure(EntityTypeBuilder<WorkspaceTypes> builder)
        {
            builder.Property(model => model.WorkspaceTypeId).ValueGeneratedNever();
            builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.WorkspaceTypeName);
            builder.HasData(Enumeration.GetAll<WorkspaceType>().Select(value =>
                new WorkspaceTypes { WorkspaceTypeId = value.Id, Name = value.Name }));
        }
    }
}