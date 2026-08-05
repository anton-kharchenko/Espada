using Espada.Db.Constants;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable(DbTableConstants.Projects, DbConstants.SchemaName);
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).HasColumnName("ProjectId")
                .HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => ProjectId.Create(value)).ValueGeneratedNever();
            builder.Property(entity => entity.WorkspaceId).HasColumnType(DbIdentifierColumnTypeConstants.Uuid)
                .HasConversion(id => id.Value, value => WorkspaceId.Create(value)).IsRequired();
            builder.Property(entity => entity.Name).HasColumnType(DbTextColumnTypeConstants.Varchar200)
                .HasMaxLength(DbMaxLengthConstants.L200).IsRequired();
            builder.Property(entity => entity.CanonicalRemoteUri).HasColumnType(DbTextColumnTypeConstants.Varchar2048)
                .HasMaxLength(DbMaxLengthConstants.L2048).IsRequired(false);
            builder.Property(entity => entity.LocalAliases).HasColumnType(DbTextColumnTypeConstants.TextArray)
                .IsRequired();
            builder.Property(entity => entity.CreatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.UpdatedAtUtc).HasColumnType(DbDateTimeColumnTypeConstants.TimestampTz)
                .IsRequired();
            builder.Property(entity => entity.Version).IsRowVersion();
            builder.HasAlternateKey(entity => new { entity.Id, entity.WorkspaceId });
            builder.HasOne<Workspace>().WithMany().HasForeignKey(entity => entity.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(entity => new { entity.WorkspaceId, entity.CanonicalRemoteUri }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ProjectWorkspaceRemote);
        }
    }
}