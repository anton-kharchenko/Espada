namespace Espada.Billing.Models
{
    public sealed record StripeWebhookReceipt(bool Received, bool Duplicate);
}