using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class ArtifactRevisionConfiguration : IEntityTypeConfiguration<Models.ArtifactRevisions>
{
    public void Configure(EntityTypeBuilder<Models.ArtifactRevisions> builder)
    {
        builder.Property(model => model.ArtifactRevisionId).ValueGeneratedNever();
        builder.HasOne<Models.Artifacts>().WithMany().HasForeignKey(model => model.ArtifactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ArtifactRevisions_ArtifactId");
        builder.HasIndex(model => new { model.ArtifactId, model.RevisionNumber }).IsUnique().HasDatabaseName(DbIndexConstants.ArtifactRevisionArtifactNumber);
    }
}
