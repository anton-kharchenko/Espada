using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class AgentSessionConfiguration : IEntityTypeConfiguration<AgentSession>
    {
        public void Configure(EntityTypeBuilder<AgentSession> builder)
        {
            builder.ToTable(DbTableConstants.AgentSessions, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("AgentSessionId").HasConversion(id => id.Value, value => AgentSessionId.Create(value))
                .ValueGeneratedNever();
            builder.Property(entity => entity.WorkspaceId)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value));
            builder.Property(entity => entity.ProjectId)
                .HasConversion(id => id.Value, value => ProjectId.Create(value));
            builder.Property(entity => entity.AgentProfileId)
                .HasConversion(id => id.Value, value => AgentProfileId.Create(value));
            builder.Property(entity => entity.DeviceId)
                .HasConversion(id => id.Value, value => DeviceId.Create(value));
            builder.Property(entity => entity.Prompt).HasColumnType(DbTextColumnTypeConstants.TextType);
            builder.Property(entity => entity.BranchName).HasMaxLength(DbMaxLengthConstants.L255);
            builder.Property(entity => entity.WorktreePath).HasMaxLength(DbMaxLengthConstants.L2048);
            builder.Property(entity => entity.Status).HasColumnName("StatusId")
                .HasConversion(type => type.Id, value => Enumeration.FromId<AgentSessionStatusType>(value));
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Project>().WithMany()
                .HasForeignKey(entity => new { entity.ProjectId, entity.WorkspaceId })
                .HasPrincipalKey(entity => new { entity.Id, entity.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AgentProfile>().WithMany().HasForeignKey(entity => entity.AgentProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Device>().WithMany().HasForeignKey(entity => entity.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.CreatedAtUtc });
        }
    }
}