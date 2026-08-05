using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class IngestionJobConfiguration : IEntityTypeConfiguration<IngestionJobs>
    {
        public void Configure(EntityTypeBuilder<IngestionJobs> builder)
        {
            builder.Property(job => job.JobId).ValueGeneratedNever();
            builder.HasIndex(job => job.IdempotencyKey).IsUnique();
            builder.HasIndex(job => new { job.Status, job.AvailableAtUtc, job.LeaseExpiresAtUtc });
        }
    }
}