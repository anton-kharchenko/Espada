using Espada.Db.Constants;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ArtifactConfiguration : IEntityTypeConfiguration<Models.Artifacts>
{
    public void Configure(EntityTypeBuilder<Models.Artifacts> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            DbConstraintConstants.ArtifactPriorityRange,
            CheckConstraintSql.ContextPriority(nameof(Models.Artifacts.Priority))));
        builder.Property(model => model.ArtifactId).ValueGeneratedNever();
        builder.Property(model => model.Priority).HasDefaultValue(ContextPriority.Neutral.Value);
        builder.Property(model => model.Version).IsRowVersion();
        builder.HasOne<Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.ArtifactTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.ArtifactStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Artifacts_WorkspaceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Artifacts_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.Title }).HasDatabaseName("IX_Artifacts_WorkspaceId_Title");
    }
}
