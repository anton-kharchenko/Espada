using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ArtifactRepositorySpy
        : IArtifactRepository
    {
        public Artifact? AddedArtifact { get; private set; }

        public Artifact? ArtifactToReturn { get; set; }

        public int AddCallCount { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public ArtifactId? ReceivedArtifactId { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public CancellationToken GetByIdCancellationToken { get; private set; }

        public Task AddAsync(
            Artifact artifact,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifact);

            AddedArtifact = artifact;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Artifact?> GetByIdAsync(
            ArtifactId artifactId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactId);

            ReceivedArtifactId = artifactId;
            GetByIdCallCount++;
            GetByIdCancellationToken = cancellationToken;

            return Task.FromResult(ArtifactToReturn);
        }
    }
}