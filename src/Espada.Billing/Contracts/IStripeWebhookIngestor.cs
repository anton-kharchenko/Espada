namespace Espada.Billing.Contracts;

public interface IStripeWebhookIngestor
{
    Task<bool> AcceptAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);
}