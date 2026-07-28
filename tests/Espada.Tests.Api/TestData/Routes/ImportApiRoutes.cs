namespace Espada.Tests.Api.TestData.Routes
{
    internal static class ImportApiRoutes
    {
        public static string Request(Guid workspaceId)
        {
            return $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/imports";
        }
    }
}