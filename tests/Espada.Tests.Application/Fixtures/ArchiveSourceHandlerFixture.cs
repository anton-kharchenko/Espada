using Espada.Application.UseCases.Sources.Commands.ArchiveSource;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class ArchiveSourceHandlerFixture
    {
        public SourceRepositorySpy SourceRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.SourceArchivedAtUtc);

        public ArchiveSourceCommandHandler CreateHandler() => new(SourceRepository, UnitOfWork, ClockService);

        public Source GivenActiveSourceExists(WorkspaceId? workspaceId = null)
        {
            Source source = new SourceBuilder().InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId).BuildWithoutPendingEvents();

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public Source GivenArchivedSourceExists(DateTimeOffset? archivedAtUtc = null)
        {
            Source source = new SourceBuilder().BuildArchivedWithoutPendingEvents(archivedAtUtc);

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public void GivenSourceDoesNotExist() => SourceRepository.SourceToReturn = null;
    }
}