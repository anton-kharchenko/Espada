using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetWorkspaceByIdHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public GetWorkspaceByIdQueryHandler CreateHandler() => new(WorkspaceRepository);

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist() => WorkspaceRepository.WorkspaceToReturn = null;
    }
}