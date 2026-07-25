namespace Espada.Tests.Api.TestData;

internal static class ApiRoutes
{
    private const string ApiV1 = "/api/v1";

    public static class System
    {
        public const string Get = ApiV1 + "/system";
    }

    public static class Sources
    {
        public static string Register(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}/sources";

        public static string GetById(Guid workspaceId, Guid sourceId) => $"{ApiV1}/workspaces/{workspaceId}/sources/{sourceId}";

        public static string Archive(Guid workspaceId, Guid sourceId) => $"{ApiV1}/workspaces/{workspaceId}/sources/{sourceId}/archive";
    }

    public static class Imports
    {
        public static string Request(Guid workspaceId, Guid sourceId) => $"{ApiV1}/workspaces/{workspaceId}/imports/sources/{sourceId}";

        public static string Start(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/start";

        public static string Complete(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/complete";

        public static string Fail(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/fail";

        public static string Cancel(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/cancel";

        public static string GetById(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}";
    }

    public static class Artifacts
    {
        public static string Create(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts";

        public static string List(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts";

        public static string GetById(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}";

        public static string Rename(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/rename";

        public static string Archive(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/archive";
    }

    public static class ArtifactRevisions
    {
        public static string Add(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions";

        public static string List(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions";

        public static string GetById(Guid workspaceId, Guid artifactId, Guid artifactRevisionId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions/{artifactRevisionId}";
    }
}