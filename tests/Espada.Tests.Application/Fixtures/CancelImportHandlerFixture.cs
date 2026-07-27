using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures;

internal sealed class CancelImportHandlerFixture
{
    public ImportJobRepositorySpy ImportJobRepository { get; } = new();

    public UnitOfWorkSpy UnitOfWork { get; } = new();

    public TestClockService ClockService { get; } = new(TestDates.ImportCancelledAtUtc);

    public JobQueueSpy JobQueue { get; } = new();

    public CancelImportCommandHandler CreateHandler() => new(ImportJobRepository, UnitOfWork, ClockService, JobQueue);

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

    public ImportJob GivenSucceededImportExists()
    {
        ImportJob importJob = new ImportJobBuilder()
            .BuildSucceededWithoutPendingEvents();

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

    public ImportJob GivenCancelledImportExists()
    {
        ImportJob importJob = new ImportJobBuilder()
            .BuildCancelledFromRequestedWithoutPendingEvents();

        ImportJobRepository.ImportJobToReturn = importJob;

        return importJob;
    }

    public void GivenImportDoesNotExist() => ImportJobRepository.ImportJobToReturn = null;
}