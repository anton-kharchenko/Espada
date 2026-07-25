using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ListArtifactRevisionsHandlerFixture
    {
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();

        public ListArtifactRevisionsQueryHandler CreateHandler()
        {
            return new ListArtifactRevisionsQueryHandler(
                ArtifactRepository,
                ArtifactRevisionRepository);
        }

        public (
            Artifact Artifact,
            ArtifactRevision FirstRevision,
            ArtifactRevision SecondRevision)
            GivenArtifactWithTwoRevisions(
                WorkspaceId? workspaceId = null)
        {
            Artifact artifact = new ArtifactBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            ArtifactRevision firstRevision = ArtifactRevisionFactory.Create(
                artifact,
                ArtifactTestIds.FirstRevisionId,
                ArtifactTestValues.FirstContent,
                ArtifactTestDates.FirstRevisionCreatedAtUtc);

            ArtifactRevision secondRevision = ArtifactRevisionFactory.Create(
                artifact,
                ArtifactTestIds.SecondRevisionId,
                ArtifactTestValues.SecondContent,
                ArtifactTestDates.SecondRevisionCreatedAtUtc);

            ArtifactRepository.ArtifactToReturn = artifact;

            ArtifactRevisionRepository.RevisionsToReturn =
                new[]
                {
                    firstRevision,
                    secondRevision
                };

            return (
                artifact,
                firstRevision,
                secondRevision);
        }

        public Artifact GivenArtifactWithoutRevisions()
        {
            Artifact artifact = new ArtifactBuilder()
                .BuildWithoutPendingEvents();

            ArtifactRepository.ArtifactToReturn = artifact;

            ArtifactRevisionRepository.RevisionsToReturn =
                Array.Empty<ArtifactRevision>();

            return artifact;
        }

        public void GivenArtifactDoesNotExist()
        {
            ArtifactRepository.ArtifactToReturn = null;
        }
    }
}