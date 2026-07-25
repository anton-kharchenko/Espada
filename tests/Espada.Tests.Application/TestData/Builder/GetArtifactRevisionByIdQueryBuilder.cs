using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class GetArtifactRevisionByIdQueryBuilder
    {
        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        private Guid _artifactId =
            TestIds.DefaultArtifactId.Value;

        private Guid _artifactRevisionId =
            TestIds.DefaultArtifactRevisionId.Value;

        public GetArtifactRevisionByIdQueryBuilder InWorkspace(
            Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public GetArtifactRevisionByIdQueryBuilder ForArtifact(
            Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public GetArtifactRevisionByIdQueryBuilder ForRevision(
            Guid artifactRevisionId)
        {
            _artifactRevisionId = artifactRevisionId;
            return this;
        }

        public GetArtifactRevisionByIdQuery Build()
        {
            return new GetArtifactRevisionByIdQuery(
                _workspaceId,
                _artifactId,
                _artifactRevisionId);
        }
    }
}