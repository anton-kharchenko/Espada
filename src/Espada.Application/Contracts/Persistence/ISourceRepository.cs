using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface ISourceRepository
    {
        Task AddAsync(Source source, CancellationToken cancellationToken = default);

        Task<Source?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Source>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
    }
}