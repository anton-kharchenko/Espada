using Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ArchiveWorkspaceHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClock Clock { get; } = new(TestDates.WorkspaceArchivedAtUtc);

        public ArchiveWorkspaceCommandHandler CreateHandler() => new(WorkspaceRepository, UnitOfWork, Clock);

        public Workspace GivenActiveWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public Workspace GivenArchivedWorkspaceExists(DateTimeOffset? archivedAtUtc = null)
        {
            Workspace workspace = new WorkspaceBuilder().BuildArchivedWithoutPendingEvents(archivedAtUtc);

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist()
        {
            WorkspaceRepository.WorkspaceToReturn = null;
        }
    }
}