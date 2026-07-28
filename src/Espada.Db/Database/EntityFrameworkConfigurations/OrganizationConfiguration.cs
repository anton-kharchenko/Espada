using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organizations>
    {
        public void Configure(EntityTypeBuilder<Organizations> builder)
        {
            builder.Property(model => model.OrganizationId).ValueGeneratedNever();
            builder.Property(model => model.Version).IsRowVersion();
            builder.HasIndex(model => model.Name).HasDatabaseName("IX_Organizations_Name");
        }
    }
}