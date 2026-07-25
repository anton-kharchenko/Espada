using Espada.Application.UseCases.Sources.Queries.GetSourceById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetSourceByIdHandlerFixture
    {
        public SourceRepositorySpy SourceRepository { get; } = new();

        public GetSourceByIdQueryHandler CreateHandler() => new(SourceRepository);

        public Source GivenSourceExists(WorkspaceId? workspaceId = null)
        {
            Source source = new SourceBuilder().InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId).BuildWithoutPendingEvents();

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public void GivenSourceDoesNotExist() => SourceRepository.SourceToReturn = null;
    }
}