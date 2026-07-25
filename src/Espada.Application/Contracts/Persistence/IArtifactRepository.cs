using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IArtifactRepository
    {
        Task AddAsync(
            Artifact artifact,
            CancellationToken cancellationToken = default);

        Task<Artifact?> GetByIdAsync(
            ArtifactId artifactId,
            CancellationToken cancellationToken = default);
    }
}