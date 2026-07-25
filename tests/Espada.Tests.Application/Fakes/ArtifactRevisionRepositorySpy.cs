using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ArtifactRevisionRepositorySpy
        : IArtifactRevisionRepository
    {
        public ArtifactRevision? AddedArtifactRevision { get; private set; }

        public ArtifactRevision? ArtifactRevisionToReturn { get; set; }

        public IReadOnlyList<ArtifactRevision> RevisionsToReturn { get; set; } =
            Array.Empty<ArtifactRevision>();

        public int AddCallCount { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public int ListByArtifactIdCallCount { get; private set; }

        public ArtifactRevisionId? ReceivedArtifactRevisionId { get; private set; }

        public ArtifactId? ReceivedArtifactId { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public CancellationToken GetByIdCancellationToken { get; private set; }

        public CancellationToken ListCancellationToken { get; private set; }

        public Task AddAsync(
            ArtifactRevision artifactRevision,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevision);

            AddedArtifactRevision = artifactRevision;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<ArtifactRevision?> GetByIdAsync(
            ArtifactRevisionId artifactRevisionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevisionId);

            ReceivedArtifactRevisionId = artifactRevisionId;
            GetByIdCallCount++;
            GetByIdCancellationToken = cancellationToken;

            return Task.FromResult(ArtifactRevisionToReturn);
        }

        public Task<IReadOnlyList<ArtifactRevision>> ListByArtifactIdAsync(
            ArtifactId artifactId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactId);

            ReceivedArtifactId = artifactId;
            ListByArtifactIdCallCount++;
            ListCancellationToken = cancellationToken;

            return Task.FromResult(RevisionsToReturn);
        }
    }
}