namespace Espada.Tests.Api.TestData.Routes
{
    internal static class SourceApiRoutes
    {
        public static string Register(Guid workspaceId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/sources";
        }
    }
}