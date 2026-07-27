using Espada.Db.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations;

internal sealed class IngestionJobConfiguration : IEntityTypeConfiguration<Models.IngestionJobs>
{
    public void Configure(EntityTypeBuilder<Models.IngestionJobs> builder)
    {
        builder.Property(job => job.JobId).ValueGeneratedNever();
        builder.HasIndex(job => job.IdempotencyKey).IsUnique();
        builder.HasIndex(job => new { job.Status, job.AvailableAtUtc, job.LeaseExpiresAtUtc });
        builder.HasOne<Models.ImportJobs>()
            .WithMany()
            .HasForeignKey(job => job.ImportJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}