namespace Espada.Tests.Api.TestData.Routes
{
    internal static class ArtifactApiRoutes
    {
        public static string Create(Guid workspaceId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/artifacts";
        }

        public static string Rename(Guid workspaceId, Guid artifactId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/rename";
        }
    }
}