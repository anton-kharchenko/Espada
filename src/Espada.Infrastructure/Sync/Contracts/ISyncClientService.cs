using Espada.Infrastructure.Sync.Client;

namespace Espada.Infrastructure.Sync.Contracts
{
    public interface ISyncClientService
    {
        bool IsConfigured { get; }

        Task<SyncCycleResponse> RunAsync(
            CancellationToken cancellationToken);
    }
}