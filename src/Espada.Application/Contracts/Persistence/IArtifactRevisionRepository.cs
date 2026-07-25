using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IArtifactRevisionRepository
    {
        Task AddAsync(
            ArtifactRevision artifactRevision,
            CancellationToken cancellationToken = default);

        Task<ArtifactRevision?> GetByIdAsync(
            ArtifactRevisionId artifactRevisionId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ArtifactRevision>> ListByArtifactIdAsync(
            ArtifactId artifactId,
            CancellationToken cancellationToken = default);
    }
}