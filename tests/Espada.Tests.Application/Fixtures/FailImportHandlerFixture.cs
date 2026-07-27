using Espada.Application.UseCases.Imports.Commands.FailImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class FailImportHandlerFixture
    {
        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.ImportFailedAtUtc);

        public FailImportCommandHandler CreateHandler() => new(ImportJobRepository, UnitOfWork, ClockService);

        public ImportJob GivenRunningImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildRunningWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenRequestedImportExists()
        {
            ImportJob importJob = new ImportJobBuilder()
                .BuildWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenFailedImportExists()
        {
            ImportJob importJob = new ImportJobBuilder()
                .BuildFailedWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public void GivenImportDoesNotExist() => ImportJobRepository.ImportJobToReturn = null;
    }
}