using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ArtifactRevisionRepositorySpy : IArtifactRevisionRepository
    {
        public ArtifactRevision? AddedArtifactRevision { get; private set; }

        public int AddCallCount { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public Task AddAsync(ArtifactRevision artifactRevision, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevision);

            AddedArtifactRevision = artifactRevision;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}