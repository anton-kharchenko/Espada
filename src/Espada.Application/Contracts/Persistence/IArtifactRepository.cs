using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence
{
    public interface IArtifactRepository
    {
        Task AddAsync(Artifact artifact, CancellationToken cancellationToken = default);
    }
}