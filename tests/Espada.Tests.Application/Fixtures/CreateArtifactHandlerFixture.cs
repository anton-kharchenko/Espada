using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class CreateArtifactHandlerFixture
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            options => options.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();
        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();
        public InstructionRuleRepositorySpy InstructionRuleRepository { get; } = new();
        public PolicyRuleRepositorySpy PolicyRuleRepository { get; } = new();
        public UnitOfWorkSpy UnitOfWork { get; } = new();
        public TestClockService ClockService { get; } = new(TestDates.ArtifactCreatedAtUtc);

        public CreateArtifactCommandHandler CreateHandler()
        {
            return new CreateArtifactCommandHandler(
                WorkspaceRepository,
                ArtifactRepository,
                ArtifactRevisionRepository,
                InstructionRuleRepository,
                PolicyRuleRepository,
                UnitOfWork,
                ClockService,
                _mapper);
        }

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();
            WorkspaceRepository.WorkspaceToReturn = workspace;
            return workspace;
        }

        public void GivenWorkspaceDoesNotExist()
        {
            WorkspaceRepository.WorkspaceToReturn = null;
        }
    }
}