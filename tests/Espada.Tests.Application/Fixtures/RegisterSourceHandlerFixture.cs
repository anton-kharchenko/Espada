using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class RegisterSourceHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public SourceRepositorySpy SourceRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.UtcNow);

        public RegisterSourceCommandHandler CreateHandler()
        {
            return new RegisterSourceCommandHandler(WorkspaceRepository, SourceRepository, UnitOfWork, ClockService);
        }

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist()
        {
            WorkspaceRepository.WorkspaceToReturn = null;
        }
    }
}