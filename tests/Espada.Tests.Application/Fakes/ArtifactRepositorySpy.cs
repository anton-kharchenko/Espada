using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ArtifactRepositorySpy : IArtifactRepository
    {
        public Artifact? AddedArtifact { get; private set; }

        public int AddCallCount { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public Task AddAsync(Artifact artifact, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifact);

            AddedArtifact = artifact;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}