namespace Espada.Billing.Contracts
{
    public interface IUsageReconciliationProcessor
    {
        Task<bool> ProcessNextAsync(
            string workerId,
            CancellationToken cancellationToken = default);
    }
}