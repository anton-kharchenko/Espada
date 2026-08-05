using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentProfileConfiguration : IEntityTypeConfiguration<AgentProfile>
    {
        public void Configure(EntityTypeBuilder<AgentProfile> builder)
        {
            builder.ToTable(DbTableConstants.AgentProfiles, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("AgentProfileId").HasConversion(id => id.Value, value => AgentProfileId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value));
            builder.Property(entity => entity.Vendor).HasColumnName("VendorTypeId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<AgentVendorType>(value));
            builder.Property(entity => entity.Name).HasMaxLength(DbMaxLengthConstants.L200);
            builder.Property(entity => entity.SettingsJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb);
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.Vendor, entity.Name }).IsUnique()
                .HasDatabaseName(DbIndexConstants.AgentProfileWorkspaceVendorName);
        }
    }
}