using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;

namespace Espada.Tests.Application.TestData.Builder
{
    internal sealed class ListArtifactsQueryBuilder
    {
        private Guid _workspaceId =
            TestIds.DefaultWorkspaceId.Value;

        public ListArtifactsQueryBuilder InWorkspace(Guid workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ListArtifactsQuery Build()
        {
            return new ListArtifactsQuery(_workspaceId);
        }
    }
}