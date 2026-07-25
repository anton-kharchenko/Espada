using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetArtifactRevisionByIdHandlerFixture
    {
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();

        public GetArtifactRevisionByIdQueryHandler CreateHandler()
        {
            return new GetArtifactRevisionByIdQueryHandler(
                ArtifactRepository,
                ArtifactRevisionRepository);
        }

        public (Artifact Artifact, ArtifactRevision Revision)
            GivenRevisionExists(WorkspaceId? workspaceId = null)
        {
            Artifact artifact = new ArtifactBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            ArtifactRevision revision = ArtifactRevisionFactory.Create(
                artifact,
                ArtifactTestIds.FirstRevisionId,
                ArtifactTestValues.FirstContent,
                ArtifactTestDates.FirstRevisionCreatedAtUtc);

            ArtifactRepository.ArtifactToReturn = artifact;
            ArtifactRevisionRepository.ArtifactRevisionToReturn = revision;

            return (artifact, revision);
        }

        public (
            Artifact RequestedArtifact,
            ArtifactRevision ForeignRevision)
            GivenRevisionBelongsToAnotherArtifact()
        {
            Artifact requestedArtifact = new ArtifactBuilder()
                .BuildWithoutPendingEvents();

            Artifact anotherArtifact = new ArtifactBuilder()
                .WithId(ArtifactTestIds.AnotherArtifactId)
                .BuildWithoutPendingEvents();

            ArtifactRevision foreignRevision = ArtifactRevisionFactory.Create(
                anotherArtifact,
                ArtifactTestIds.FirstRevisionId,
                ArtifactTestValues.FirstContent,
                ArtifactTestDates.FirstRevisionCreatedAtUtc);

            ArtifactRepository.ArtifactToReturn = requestedArtifact;
            ArtifactRevisionRepository.ArtifactRevisionToReturn =
                foreignRevision;

            return (requestedArtifact, foreignRevision);
        }

        public void GivenArtifactDoesNotExist()
        {
            ArtifactRepository.ArtifactToReturn = null;
        }

        public Artifact GivenRevisionDoesNotExist()
        {
            Artifact artifact = new ArtifactBuilder()
                .BuildWithoutPendingEvents();

            ArtifactRepository.ArtifactToReturn = artifact;
            ArtifactRevisionRepository.ArtifactRevisionToReturn = null;

            return artifact;
        }
    }
}