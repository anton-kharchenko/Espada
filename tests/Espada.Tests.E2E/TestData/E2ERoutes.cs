using Espada.Tests.E2E.TestData.Constants;

namespace Espada.Tests.E2E.TestData
{
    internal static class E2ERoutes
    {
        public static string Workspace(Guid workspaceId)
        {
            return $"{E2ERouteConstants.Workspaces}/{workspaceId}";
        }

        public static string Sources(Guid workspaceId)
        {
            return $"{Workspace(workspaceId)}/sources";
        }

        public static string Source(Guid workspaceId, Guid sourceId)
        {
            return $"{Sources(workspaceId)}/{sourceId}";
        }

        public static string SourcePriority(Guid workspaceId, Guid sourceId)
        {
            return $"{Source(workspaceId, sourceId)}/priority";
        }

        public static string RequestImport(Guid workspaceId)
        {
            return $"{Workspace(workspaceId)}/imports";
        }

        public static string Import(Guid workspaceId, Guid importJobId)
        {
            return $"{Workspace(workspaceId)}/imports/{importJobId}";
        }

        public static string CancelImport(Guid workspaceId, Guid importJobId)
        {
            return $"{Import(workspaceId, importJobId)}/cancel";
        }

        public static string Artifacts(Guid workspaceId)
        {
            return $"{Workspace(workspaceId)}/artifacts";
        }

        public static string Artifact(Guid workspaceId, Guid artifactId)
        {
            return $"{Artifacts(workspaceId)}/{artifactId}";
        }

        public static string ArtifactPriority(Guid workspaceId, Guid artifactId)
        {
            return $"{Artifact(workspaceId, artifactId)}/priority";
        }

        public static string RenameArtifact(Guid workspaceId, Guid artifactId)
        {
            return $"{Artifact(workspaceId, artifactId)}/rename";
        }

        public static string ArchiveArtifact(Guid workspaceId, Guid artifactId)
        {
            return $"{Artifact(workspaceId, artifactId)}/archive";
        }

        public static string Revisions(Guid workspaceId, Guid artifactId)
        {
            return $"{Artifact(workspaceId, artifactId)}/revisions";
        }

        public static string ChunkBatches(Guid workspaceId, Guid artifactId, Guid revisionId)
        {
            return $"{Revisions(workspaceId, artifactId)}/{revisionId}/chunk-batches";
        }

        public static string CreateChunks(Guid workspaceId, Guid chunkBatchId)
        {
            return $"{Workspace(workspaceId)}/chunk-batches/{chunkBatchId}/chunks";
        }

        public static string ChunksByRevision(Guid workspaceId, Guid revisionId)
        {
            return $"{Workspace(workspaceId)}/artifact-revisions/{revisionId}/chunks";
        }

        public static string Chunk(Guid workspaceId, Guid chunkId)
        {
            return $"{Workspace(workspaceId)}/chunks/{chunkId}";
        }

        public static string Embedding(Guid workspaceId, Guid chunkId)
        {
            return $"{Chunk(workspaceId, chunkId)}/embedding";
        }

        public static string EmbeddingByModel(
            Guid workspaceId,
            Guid chunkId,
            string modelIdentifier,
            string modelVersion)
        {
            return $"{Embedding(workspaceId, chunkId)}" +
                   $"?modelIdentifier={Uri.EscapeDataString(modelIdentifier)}" +
                   $"&modelVersion={Uri.EscapeDataString(modelVersion)}";
        }
    }
}