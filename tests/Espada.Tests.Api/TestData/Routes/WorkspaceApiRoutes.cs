namespace Espada.Tests.Api.TestData.Routes
{
    internal static class WorkspaceApiRoutes
    {
        public static string GetById(Guid workspaceId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}";
        }
    }
}