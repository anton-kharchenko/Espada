namespace Espada.Tests.Api.TestData.Routes;

internal static class SourceApiRoutes
{
    public static string Register(Guid workspaceId) => $"{ApiRouteConstants.ApiV1}/workspaces/{workspaceId}/sources";
}