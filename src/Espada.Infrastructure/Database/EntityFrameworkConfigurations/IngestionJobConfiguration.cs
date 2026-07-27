using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class IngestionJobConfiguration : IEntityTypeConfiguration<Espada.Db.Models.IngestionJobs>
{
    public void Configure(EntityTypeBuilder<Espada.Db.Models.IngestionJobs> builder)
    {
        builder.Property(job => job.JobId).ValueGeneratedNever();
        builder.HasIndex(job => job.IdempotencyKey).IsUnique();
        builder.HasIndex(job => new { job.Status, job.AvailableAtUtc, job.LeaseExpiresAtUtc });
        builder.HasOne<Espada.Db.Models.ImportJobs>()
            .WithMany()
            .HasForeignKey(job => job.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}