using Espada.Application.UseCases.Imports.Commands.CompleteImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class CompleteImportHandlerFixture
    {
        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClock Clock { get; } = new(TestDates.ImportCompletedAtUtc);

        public CompleteImportCommandHandler CreateHandler() => new(ImportJobRepository, UnitOfWork, Clock);

        public ImportJob GivenRunningImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                    .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                    .BuildRunningWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenRequestedImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                    .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                    .BuildWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public ImportJob GivenSucceededImportExists(WorkspaceId? workspaceId = null)
        {
            ImportJob importJob = new ImportJobBuilder()
                    .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                    .BuildSucceededWithoutPendingEvents();

            ImportJobRepository.ImportJobToReturn = importJob;

            return importJob;
        }

        public void GivenImportDoesNotExist() => ImportJobRepository.ImportJobToReturn = null;
    }
}