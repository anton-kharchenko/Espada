using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable(DbTableConstants.Organizations, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("OrganizationId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => OrganizationId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.Name).HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200).IsRequired();
            builder.Property(entity => entity.CreatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasIndex(entity => entity.Name).HasDatabaseName("IX_Organizations_Name");
        }
    }
}