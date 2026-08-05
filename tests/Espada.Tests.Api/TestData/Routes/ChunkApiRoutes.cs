namespace Espada.Tests.Api.TestData.Routes
{
    internal static class ChunkApiRoutes
    {
        public static string Create(Guid workspaceId, Guid chunkBatchId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/chunk-batches/{chunkBatchId}/chunks";
        }

        public static string GetById(Guid workspaceId, Guid chunkId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/chunks/{chunkId}";
        }

        public static string ListByRevision(Guid workspaceId, Guid artifactRevisionId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/artifact-revisions/{artifactRevisionId}/chunks";
        }
    }
}