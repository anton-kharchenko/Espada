using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class CreateArtifactHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClock Clock { get; } = new(TestDates.ArtifactCreatedAtUtc);

        public CreateArtifactCommandHandler CreateHandler() => new(WorkspaceRepository, ArtifactRepository, ArtifactRevisionRepository, UnitOfWork, Clock);

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist() => WorkspaceRepository.WorkspaceToReturn = null;
    }
}