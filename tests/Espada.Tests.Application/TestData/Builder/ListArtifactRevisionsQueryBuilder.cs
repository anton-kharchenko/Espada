using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ListArtifactRevisionsQueryBuilder
    {
        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        private Guid _artifactId =
            TestIds.DefaultArtifactId.Value;

        public ListArtifactRevisionsQueryBuilder InWorkspace(
            Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ListArtifactRevisionsQueryBuilder ForArtifact(
            Guid artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public ListArtifactRevisionsQuery Build()
        {
            return new ListArtifactRevisionsQuery(
                _workspaceId,
                _artifactId);
        }
    }
}