namespace Espada.Tests.E2E.TestData;

internal static class E2ERoutes
{
    private const string ApiV1 = "/api/v1";

    public const string System = ApiV1 + "/system";
    public const string OpenApi = "/openapi/v1.json";

    public static string Workspace(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}";
}