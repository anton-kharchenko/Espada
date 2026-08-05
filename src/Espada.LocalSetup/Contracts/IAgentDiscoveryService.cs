using Espada.LocalSetup.Contracts.Responses;

namespace Espada.LocalSetup.Contracts
{
    public interface IAgentDiscoveryService
    {
        Task<IReadOnlyList<LocalSetupAgentPreview>> DiscoverAsync(
            CancellationToken cancellationToken);
    }
}