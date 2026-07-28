using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class UsageReconciliationConfiguration : IEntityTypeConfiguration<UsageReconciliationOutbox>
    {
        public void Configure(EntityTypeBuilder<UsageReconciliationOutbox> builder)
        {
            builder.HasKey(message => message.EventId);
            builder.Property(message => message.EventId).ValueGeneratedNever();
            builder.Property(message => message.LeaseOwner).HasMaxLength(200);
            builder.Property(message => message.SanitizedError).HasMaxLength(1000);
            builder.HasIndex(message => message.LedgerEntryId).IsUnique();
            builder.HasIndex(message => new { message.Status, message.AvailableAtUtc, message.LeaseExpiresAtUtc });
        }
    }
}