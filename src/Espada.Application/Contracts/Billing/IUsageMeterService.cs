namespace Espada.Application.Contracts.Billing;

public interface IUsageMeterService
{
    Task RecordAsync(
        Guid workspaceId,
        string metric,
        long quantity,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}