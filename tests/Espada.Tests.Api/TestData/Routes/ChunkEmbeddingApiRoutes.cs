namespace Espada.Tests.Api.TestData.Routes;

internal static class ChunkEmbeddingApiRoutes
{
    public static string Create(Guid workspaceId, Guid chunkId) => $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/chunks/{chunkId}/embedding";
}