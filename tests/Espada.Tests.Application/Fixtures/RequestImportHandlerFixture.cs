using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class RequestImportHandlerFixture
    {
        public SourceRepositorySpy SourceRepository { get; } = new();

        public ImportJobRepositorySpy ImportJobRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClock Clock { get; } = new(TestDates.ImportRequestedAtUtc);

        public RequestImportCommandHandler CreateHandler() => new(SourceRepository, ImportJobRepository, UnitOfWork, Clock);

        public Source GivenSourceExists(WorkspaceId? workspaceId = null)
        {
            Source source = new SourceBuilder().InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId).BuildWithoutPendingEvents();

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public void GivenSourceDoesNotExist() => SourceRepository.SourceToReturn = null;
    }
}