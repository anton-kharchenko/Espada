using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class GetArtifactByIdHandlerFixture
    {
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();

        public GetArtifactByIdQueryHandler CreateHandler()
        {
            return new GetArtifactByIdQueryHandler(
                ArtifactRepository);
        }

        public Artifact GivenArtifactExists(
            WorkspaceId? workspaceId = null)
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