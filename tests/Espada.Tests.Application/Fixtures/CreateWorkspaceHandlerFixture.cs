using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class CreateWorkspaceHandlerFixture
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            options => options.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();

        public OrganizationRepositorySpy OrganizationRepository { get; } = new();

        public UnitOfWorkSpy UnitOfWork { get; } = new();

        public WorkspaceMembershipRepositorySpy MembershipRepository { get; } = new();

        public TestClockService ClockService { get; } = new(TestDates.UtcNow);

        public CreateWorkspaceCommandHandler CreateHandler()
        {
            return new CreateWorkspaceCommandHandler(
                WorkspaceRepository,
                OrganizationRepository,
                MembershipRepository,
                UnitOfWork,
                ClockService,
                _mapper);
        }
    }
}