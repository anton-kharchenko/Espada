using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class WorkspaceStatusTypesConfiguration : IEntityTypeConfiguration<WorkspaceStatusTypes>
{
    public void Configure(EntityTypeBuilder<WorkspaceStatusTypes> builder)
    {
        builder.Property(model => model.WorkspaceStatusTypeId).ValueGeneratedNever();
        builder.HasIndex(model => model.Name).IsUnique().HasDatabaseName(DbIndexConstants.WorkspaceStatusTypeName);
        builder.HasData(
            Enumeration.GetAll<WorkspaceStatusType>()
                .Select(
                    value => new WorkspaceStatusTypes
                    {
                        WorkspaceStatusTypeId = value.Id,
                        Name = value.Name
                    }));
    }
}