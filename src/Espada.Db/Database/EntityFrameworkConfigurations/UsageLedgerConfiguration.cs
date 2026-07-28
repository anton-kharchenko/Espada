using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Db.Database.EntityFrameworkConfigurations
{
    internal sealed class UsageLedgerConfiguration : IEntityTypeConfiguration<UsageLedgerEntries>
    {
        public void Configure(EntityTypeBuilder<UsageLedgerEntries> builder)
        {
            builder.HasKey(entry => entry.EntryId);
            builder.Property(entry => entry.EntryId).ValueGeneratedNever();
            builder.Property(entry => entry.Metric).HasMaxLength(100).IsRequired();
            builder.Property(entry => entry.IdempotencyKey).HasMaxLength(255).IsRequired();
            builder.HasIndex(entry => new { entry.WorkspaceId, entry.Metric, entry.IdempotencyKey }).IsUnique();
        }
    }
}