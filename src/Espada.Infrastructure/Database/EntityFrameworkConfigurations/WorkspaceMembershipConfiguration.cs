using Espada.Db.Constants;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class WorkspaceMembershipConfiguration : IEntityTypeConfiguration<WorkspaceMembership>,
        IEntityTypeConfiguration<WorkspaceMemberships>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMembership> builder)
        {
            builder.ToTable(DbTableConstants.WorkspaceMemberships, DbConstants.SchemaName);
            builder.HasKey(membership => membership.Id);
            builder.Property(membership => membership.Id)
                .HasColumnName("WorkspaceMembershipId")
                .HasConversion(id => id.Value, value => WorkspaceMembershipId.Create(value))
                .ValueGeneratedNever();
            builder.Property(membership => membership.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value))
                .IsRequired();
            builder.Property(membership => membership.Issuer)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(membership => membership.Subject)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(membership => membership.Role)
                .HasConversion(
                    role => role.Id,
                    value => Enumeration.GetAll<WorkspaceMembershipRoleType>().Single(role => role.Id == value))
                .IsRequired();
            builder.Property(membership => membership.JoinedAtUtc).IsRequired();
            builder.HasIndex(membership => new { membership.WorkspaceId, membership.Issuer, membership.Subject })
                .IsUnique();
            builder.HasOne<Workspace>()
                .WithMany()
                .HasForeignKey(membership => membership.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Ignore(membership => membership.DomainEvents);
        }

        public void Configure(EntityTypeBuilder<WorkspaceMemberships> builder)
        {
            builder.HasKey(membership => membership.WorkspaceMembershipId);
            builder.Property(membership => membership.WorkspaceMembershipId).ValueGeneratedNever();
            builder.Property(membership => membership.Issuer).HasMaxLength(500).IsRequired();
            builder.Property(membership => membership.Subject).HasMaxLength(200).IsRequired();
            builder.HasIndex(membership => new { membership.WorkspaceId, membership.Issuer, membership.Subject })
                .IsUnique();
            builder.HasOne<Workspaces>()
                .WithMany()
                .HasForeignKey(membership => membership.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}