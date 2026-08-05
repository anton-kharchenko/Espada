using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMembership>
    {
        public void Configure(EntityTypeBuilder<OrganizationMembership> builder)
        {
            builder.ToTable(DbTableConstants.OrganizationMemberships, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("OrganizationMembershipId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => OrganizationMembershipId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.OrganizationId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => OrganizationId.Create(value)).IsRequired();
            builder.Property(entity => entity.Issuer).HasColumnType(DbTextColumnTypeConstants.Varchar500)
                .HasMaxLength(DbMaxLengthConstants.L500).IsRequired();
            builder.Property(entity => entity.Subject).HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200).IsRequired();
            builder.Property(entity => entity.Role).HasColumnType(DbTextColumnTypeConstants.Varchar32)
                .HasMaxLength(DbMaxLengthConstants.L32).HasConversion(role => role.Name,
                    value => Enumeration.GetAll<OrganizationMembershipRoleType>().Single(role => role.Name == value))
                .IsRequired();
            builder.Property(entity => entity.JoinedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.HasOne<Organization>().WithMany().HasForeignKey(entity => entity.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.OrganizationId, entity.Issuer, entity.Subject }).IsUnique()
                .HasDatabaseName(DbIndexConstants.OrganizationMembershipIdentity);
        }
    }
}