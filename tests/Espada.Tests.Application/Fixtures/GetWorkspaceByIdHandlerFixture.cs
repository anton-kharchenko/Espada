using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetWorkspaceByIdHandlerFixture
    {
        private readonly IMapper _mapper = new MapperConfiguration(options => options.AddProfile<ApplicationMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public GetWorkspaceByIdQueryHandler CreateHandler() => new(WorkspaceRepository, _mapper);

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

            WorkspaceRepository.WorkspaceToReturn = workspace;

            return workspace;
        }

        public void GivenWorkspaceDoesNotExist() => WorkspaceRepository.WorkspaceToReturn = null;
    }
}