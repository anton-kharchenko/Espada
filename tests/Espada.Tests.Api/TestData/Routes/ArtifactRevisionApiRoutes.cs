namespace Espada.Tests.Api.TestData.Routes
{
    internal static class ArtifactRevisionApiRoutes
    {
        public static string Add(Guid workspaceId, Guid artifactId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions";
        }
    }
}