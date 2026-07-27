using Espada.Db.Constants;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Models.Sources>
{
    public void Configure(EntityTypeBuilder<Models.Sources> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint(
            DbConstraintConstants.SourcePriorityRange,
            CheckConstraintSql.ContextPriority(nameof(Models.Sources.Priority))));
        builder.Property(model => model.SourceId).ValueGeneratedNever();
        builder.Property(model => model.Priority).HasDefaultValue(ContextPriority.Neutral.Value);
        builder.Property(model => model.Version).IsRowVersion();
        builder.Property(model => model.DefinitionJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb).IsRequired(false);
        builder.HasOne<Models.Workspaces>().WithMany().HasForeignKey(model => model.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.SourceTypes>().WithMany().HasForeignKey(model => model.TypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Models.SourceStatusTypes>().WithMany().HasForeignKey(model => model.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.WorkspaceId).HasDatabaseName("IX_Sources_WorkspaceId");
        builder.HasIndex(model => model.StatusId).HasDatabaseName("IX_Sources_StatusId");
        builder.HasIndex(model => new { model.WorkspaceId, model.Locator }).IsUnique().HasDatabaseName(DbIndexConstants.SourceWorkspaceLocator);
    }
}
