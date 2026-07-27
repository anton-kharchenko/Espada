namespace Espada.Tests.E2E.TestData;

internal static class E2ERoutes
{
    private const string ApiV1 = "/api/v1";

    public const string Workspaces = ApiV1 + "/workspaces";
    public const string OpenApi = "/openapi/v1.json";

    public static string Workspace(Guid workspaceId) => $"{Workspaces}/{workspaceId}";
    public static string Sources(Guid workspaceId) => $"{Workspace(workspaceId)}/sources";
    public static string Source(Guid workspaceId, Guid sourceId) => $"{Sources(workspaceId)}/{sourceId}";
    public static string SourcePriority(Guid workspaceId, Guid sourceId) => $"{Source(workspaceId, sourceId)}/priority";
    public static string RequestImport(Guid workspaceId, Guid sourceId) => $"{Workspace(workspaceId)}/imports/sources/{sourceId}";
    public static string Import(Guid workspaceId, Guid importJobId) => $"{Workspace(workspaceId)}/imports/{importJobId}";
    public static string StartImport(Guid workspaceId, Guid importJobId) => $"{Import(workspaceId, importJobId)}/start";
    public static string CompleteImport(Guid workspaceId, Guid importJobId) => $"{Import(workspaceId, importJobId)}/complete";
    public static string Artifacts(Guid workspaceId) => $"{Workspace(workspaceId)}/artifacts";
    public static string Artifact(Guid workspaceId, Guid artifactId) => $"{Artifacts(workspaceId)}/{artifactId}";
    public static string ArtifactPriority(Guid workspaceId, Guid artifactId) => $"{Artifact(workspaceId, artifactId)}/priority";
    public static string RenameArtifact(Guid workspaceId, Guid artifactId) => $"{Artifact(workspaceId, artifactId)}/rename";
    public static string ArchiveArtifact(Guid workspaceId, Guid artifactId) => $"{Artifact(workspaceId, artifactId)}/archive";
    public static string Revisions(Guid workspaceId, Guid artifactId) => $"{Artifact(workspaceId, artifactId)}/revisions";
    public static string ChunkBatches(Guid workspaceId, Guid artifactId, Guid revisionId) => $"{Revisions(workspaceId, artifactId)}/{revisionId}/chunk-batches";
    public static string CreateChunks(Guid workspaceId, Guid chunkBatchId) => $"{Workspace(workspaceId)}/chunk-batches/{chunkBatchId}/chunks";
    public static string ChunksByRevision(Guid workspaceId, Guid revisionId) => $"{Workspace(workspaceId)}/artifact-revisions/{revisionId}/chunks";
    public static string Chunk(Guid workspaceId, Guid chunkId) => $"{Workspace(workspaceId)}/chunks/{chunkId}";
    public static string Embedding(Guid workspaceId, Guid chunkId) => $"{Chunk(workspaceId, chunkId)}/embedding";
    public static string ContextSearch(Guid workspaceId) => $"{Workspace(workspaceId)}/context/search";
    public static string EmbeddingByModel(Guid workspaceId, Guid chunkId, string modelIdentifier, string modelVersion) => $"{Embedding(workspaceId, chunkId)}?modelIdentifier={Uri.EscapeDataString(modelIdentifier)}&modelVersion={Uri.EscapeDataString(modelVersion)}";
}