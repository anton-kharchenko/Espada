using Espada.Domain.Rules;

namespace Espada.Billing.ApplicationErrors;

public static class BillingApplicationErrors
{
    public static DomainError Unavailable { get; } = new(
        "Billing.NotFound",
        "Billing is not available.");

    public static DomainError CustomerNotFound { get; } = new(
        "BillingCustomer.NotFound",
        "The workspace does not have a billing customer.");

    public static DomainError InvalidWebhook { get; } = new(
        "Billing.InvalidWebhook", "The Stripe webhook signature or payload is invalid.");
}