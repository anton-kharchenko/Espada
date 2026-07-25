namespace Espada.Tests.Api.TestData;

internal static class ApiRoutes
{
    private const string ApiV1 = "/api/v1";

    public static class System
    {
        public const string Get = ApiV1 + "/system";
    }

    public static class Workspaces
    {
        public static string GetById(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}";
    }

    public static class Sources
    {
        public static string Register(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}/sources";
    }

    public static class Imports
    {
        public static string Complete(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/complete";

        public static string Fail(Guid workspaceId, Guid importJobId) => $"{ApiV1}/workspaces/{workspaceId}/imports/{importJobId}/fail";
    }

    public static class Artifacts
    {
        public static string Create(Guid workspaceId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts";

        public static string Rename(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/rename";
    }

    public static class ArtifactRevisions
    {
        public static string Add(Guid workspaceId, Guid artifactId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions";
    }

    public static class ChunkBatches
    {
        public static string Create(Guid workspaceId, Guid artifactId, Guid artifactRevisionId) => $"{ApiV1}/workspaces/{workspaceId}/artifacts/{artifactId}/revisions/{artifactRevisionId}/chunk-batches";
    }

    public static class Chunks
    {
        public static string Create(Guid workspaceId, Guid chunkBatchId) => $"{ApiV1}/workspaces/{workspaceId}/chunk-batches/{chunkBatchId}/chunks";

        public static string GetById(Guid workspaceId, Guid chunkId) => $"{ApiV1}/workspaces/{workspaceId}/chunks/{chunkId}";

        public static string ListByRevision(Guid workspaceId, Guid artifactRevisionId) => $"{ApiV1}/workspaces/{workspaceId}/artifact-revisions/{artifactRevisionId}/chunks";
    }

    public static class ChunkEmbeddings
    {
        public static string Create(Guid workspaceId, Guid chunkId) => $"{ApiV1}/workspaces/{workspaceId}/chunks/{chunkId}/embedding";
    }
}