using Espada.Application.UseCases.Imports.Commands.StartImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class StartImportHandlerFixture
    {
        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.ImportStartedAtUtc);

        public StartImportCommandHandler CreateHandler()
        {
            return new StartImportCommandHandler(ImportJobRepository, UnitOfWork, ClockService);
        }

        public ImportJob GivenRequestedImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenRunningImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildRunningWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public void GivenImportDoesNotExist()
        {
            ImportJobRepository.ImportJobToReturn = null;
        }
    }
}