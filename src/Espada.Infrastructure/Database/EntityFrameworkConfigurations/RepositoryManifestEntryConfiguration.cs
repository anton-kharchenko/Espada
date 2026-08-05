using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class RepositoryManifestEntryConfiguration : IEntityTypeConfiguration<RepositoryManifestEntries>
    {
        public void Configure(EntityTypeBuilder<RepositoryManifestEntries> builder)
        {
            builder.HasKey(entity => new { entity.SourceId, entity.RelativePath });
        }
    }
}