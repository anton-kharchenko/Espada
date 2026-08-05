using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class ArtifactRevisionConfiguration : IEntityTypeConfiguration<ArtifactRevisions>
    {
        public void Configure(EntityTypeBuilder<ArtifactRevisions> builder)
        {
            builder.Property(model => model.ArtifactRevisionId).ValueGeneratedNever();
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.WorkspaceId });
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.Kind });
            builder.HasAlternateKey(model => new { model.ArtifactRevisionId, model.ArtifactId, model.Kind });
            builder.HasOne<Artifacts>().WithMany().HasForeignKey(model => new { model.ArtifactId, model.WorkspaceId })
                .HasPrincipalKey(model => new { model.ArtifactId, model.WorkspaceId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => model.ArtifactId).HasDatabaseName("IX_ArtifactRevisions_ArtifactId");
            builder.HasIndex(model => new { model.ArtifactId, model.RevisionNumber }).IsUnique()
                .HasDatabaseName(DbIndexConstants.ArtifactRevisionArtifactNumber);
        }
    }
}