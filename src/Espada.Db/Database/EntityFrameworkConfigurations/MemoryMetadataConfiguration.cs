using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class MemoryMetadataConfiguration : IEntityTypeConfiguration<MemoryMetadataRecords>
    {
        public void Configure(EntityTypeBuilder<MemoryMetadataRecords> builder)
        {
            builder.ToTable(DbTableConstants.MemoryMetadata, DbConstants.SchemaName,
                table => table.HasCheckConstraint(DbIndexConstants.MemoryMetadataKind, "\"Kind\" = 'memory'"));
            builder.Property(model => model.MemoryId).ValueGeneratedNever();
            builder.HasOne<ArtifactRevisions>().WithMany()
                .HasForeignKey(model => new { model.ArtifactRevisionId, model.ArtifactId, model.Kind })
                .HasPrincipalKey(model => new { model.ArtifactRevisionId, model.ArtifactId, model.Kind })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<MemoryMetadataRecords>().WithMany().HasForeignKey(model => model.SupersededMemoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(model => new { model.ArtifactId, model.ArtifactRevisionId }).IsUnique()
                .HasDatabaseName("UX_MemoryMetadata_ArtifactId_ArtifactRevisionId");
            builder.HasIndex(model => model.SupersededMemoryId).IsUnique()
                .HasFilter("\"SupersededMemoryId\" IS NOT NULL")
                .HasDatabaseName(DbIndexConstants.MemoryMetadataSupersededMemory);
        }
    }
}