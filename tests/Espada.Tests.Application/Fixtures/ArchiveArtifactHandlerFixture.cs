using Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ArchiveArtifactHandlerFixture
    {
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClock Clock { get; } =
            new(ArtifactTestDates.ArchivedAtUtc);

        public ArchiveArtifactCommandHandler CreateHandler()
        {
            return new ArchiveArtifactCommandHandler(
                ArtifactRepository,
                UnitOfWork,
                Clock);
        }

        public Artifact GivenArtifactExists(
            WorkspaceId? workspaceId = null)
        {
            Artifact artifact = new ArtifactBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithFirstRevisionWithoutPendingEvents();

            ArtifactRepository.ArtifactToReturn = artifact;

            return artifact;
        }

        public Artifact GivenArchivedArtifactExists()
        {
            Artifact artifact = new ArtifactBuilder()
                .BuildArchivedWithoutPendingEvents();

            ArtifactRepository.ArtifactToReturn = artifact;

            return artifact;
        }

        public void GivenArtifactDoesNotExist()
        {
            ArtifactRepository.ArtifactToReturn = null;
        }
    }
}