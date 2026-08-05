namespace Espada.Tests.Api.TestData.Routes
{
    internal static class ChunkBatchApiRoutes
    {
        public static string Create(Guid workspaceId, Guid artifactId, Guid artifactRevisionId)
        {
            return
                $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions/{artifactRevisionId}/chunk-batches";
        }
    }
}