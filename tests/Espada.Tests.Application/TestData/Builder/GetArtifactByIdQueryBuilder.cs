using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class GetArtifactByIdQueryBuilder
    {
        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        private Guid _artifactId =
            ArtifactTestIds.DefaultArtifactId.Value;

        public GetArtifactByIdQueryBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public GetArtifactByIdQueryBuilder ForArtifact(Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public GetArtifactByIdQuery Build()
        {
            return new GetArtifactByIdQuery(
                _workspaceId,
                _artifactId);
        }
    }
}