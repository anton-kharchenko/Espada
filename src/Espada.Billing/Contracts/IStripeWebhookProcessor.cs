namespace Espada.Billing.Contracts
{
    public interface IStripeWebhookProcessor
    {
        Task<bool> ProcessNextAsync(
            string workerId,
            CancellationToken cancellationToken = default);
    }
}