using Espada.Db.Constants;
using Espada.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Espada.Infrastructure.Database.EntityFrameworkConfigurations
{
    internal sealed class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvents>
    {
        public void Configure(EntityTypeBuilder<PaymentEvents> builder)
        {
            builder.HasKey(paymentEvent => paymentEvent.ProviderEventId);
            builder.Property(paymentEvent => paymentEvent.ProviderEventId).HasMaxLength(255).ValueGeneratedNever();
            builder.Property(paymentEvent => paymentEvent.EventType).HasMaxLength(200).IsRequired();
            builder.Property(paymentEvent => paymentEvent.ApiVersion).HasMaxLength(50).IsRequired();
            builder.Property(paymentEvent => paymentEvent.PayloadJson).HasColumnType(DbJsonColumnTypeConstants.Jsonb)
                .IsRequired();
            builder.Property(paymentEvent => paymentEvent.LeaseOwner).HasMaxLength(200);
            builder.Property(paymentEvent => paymentEvent.SanitizedError).HasMaxLength(1000);
            builder.HasIndex(paymentEvent => new
            {
                paymentEvent.Status, paymentEvent.AvailableAtUtc, paymentEvent.LeaseExpiresAtUtc
            });
        }
    }
}