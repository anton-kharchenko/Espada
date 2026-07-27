using Espada.Application.Contracts.Billing;

namespace Espada.Application.Services.Billing;

internal sealed class NoOpUsageMeterService : IUsageMeterService
{
    public Task RecordAsync(
        Guid workspaceId,
        string metric,
        long quantity,
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}