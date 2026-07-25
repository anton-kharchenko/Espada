using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence
{
    public interface IArtifactRevisionRepository
    {
        Task AddAsync(ArtifactRevision artifactRevision, CancellationToken cancellationToken = default);
    }
}