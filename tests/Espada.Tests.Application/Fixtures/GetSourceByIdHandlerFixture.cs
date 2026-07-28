using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Sources.Queries.GetSourceById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetSourceByIdHandlerFixture
    {
        private readonly IMapper _mapper =
            new MapperConfiguration(options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();

        public SourceRepositorySpy SourceRepository { get; } = new();

        public GetSourceByIdQueryHandler CreateHandler()
        {
            return new GetSourceByIdQueryHandler(SourceRepository, _mapper);
        }

        public Source GivenSourceExists(WorkspaceId? workspaceId = null)
        {
            Source source = new SourceBuilder().InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithoutPendingEvents();

            SourceRepository.SourceToReturn = source;

            return source;
        }

        public void GivenSourceDoesNotExist()
        {
            SourceRepository.SourceToReturn = null;
        }
    }
}