using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetArtifactByIdHandlerFixture
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            options => options.AddProfile<ApplicationMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();

        public ArtifactRepositorySpy ArtifactRepository { get; } = new();
        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();
        public InstructionRuleRepositorySpy InstructionRuleRepository { get; } = new();
        public PolicyRuleRepositorySpy PolicyRuleRepository { get; } = new();

        public GetArtifactByIdQueryHandler CreateHandler()
        {
            return new GetArtifactByIdQueryHandler(
                ArtifactRepository,
                ArtifactRevisionRepository,
                InstructionRuleRepository,
                PolicyRuleRepository,
                _mapper);
        }

        public Artifact GivenArtifactExists(WorkspaceId? workspaceId = null)
        {
            Artifact artifact = new ArtifactBuilder()
                .InWorkspace(workspaceId ?? TestIds.DefaultWorkspaceId)
                .BuildWithFirstRevisionWithoutPendingEvents();
            ArtifactRepository.ArtifactToReturn = artifact;
            return artifact;
        }

        public void GivenArtifactDoesNotExist()
        {
            ArtifactRepository.ArtifactToReturn = null;
        }
    }
}