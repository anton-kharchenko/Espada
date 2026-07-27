using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class BillingSetupRelationshipConfiguration : IEntityTypeConfiguration<BillingCustomers>, IEntityTypeConfiguration<UsageLedgerEntries>, IEntityTypeConfiguration<UsageReconciliationOutbox>
{
    public void Configure(EntityTypeBuilder<BillingCustomers> builder) =>
        builder.HasOne<Workspaces>()
            .WithMany()
            .HasForeignKey(customer => customer.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

    public void Configure(EntityTypeBuilder<UsageLedgerEntries> builder) =>
        builder.HasOne<Workspaces>()
            .WithMany()
            .HasForeignKey(entry => entry.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

    public void Configure(EntityTypeBuilder<UsageReconciliationOutbox> builder) =>
        builder.HasOne<UsageLedgerEntries>()
            .WithMany()
            .HasForeignKey(message => message.LedgerEntryId)
            .OnDelete(DeleteBehavior.Restrict);
}