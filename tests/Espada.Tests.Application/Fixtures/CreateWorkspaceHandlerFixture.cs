using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.Fixtures;

internal sealed class CreateWorkspaceHandlerFixture
{
    public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

    public UnitOfWorkSpy UnitOfWork { get; } = new();
    public WorkspaceMembershipRepositorySpy MembershipRepository { get; } = new();

    public TestClockService ClockService { get; } = new(TestDates.UtcNow);

    public CreateWorkspaceCommandHandler CreateHandler() => new(WorkspaceRepository, MembershipRepository, UnitOfWork, ClockService);
}