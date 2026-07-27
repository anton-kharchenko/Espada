using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations;

internal sealed class BillingCustomerConfiguration : IEntityTypeConfiguration<BillingCustomers>
{
    public void Configure(EntityTypeBuilder<BillingCustomers> builder)
    {
        builder.HasKey(customer => customer.WorkspaceId);
        builder.Property(customer => customer.ProviderCustomerId).HasMaxLength(255).IsRequired();
        builder.Property(customer => customer.ProviderSubscriptionId).HasMaxLength(255);
        builder.Property(customer => customer.SubscriptionStatus).HasMaxLength(100).IsRequired();
        builder.HasIndex(customer => customer.ProviderCustomerId).IsUnique();
    }
}