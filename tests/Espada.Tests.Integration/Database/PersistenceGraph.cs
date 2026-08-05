using Espada.Domain.Aggregates;

namespace Espada.Tests.Integration.Database
{
    internal sealed record PersistenceGraph(
        Workspace Workspace,
        Source Source,
        ImportJob ImportJob,
        Artifact Artifact,
        ArtifactRevision ArtifactRevision,
        ChunkBatch ChunkBatch,
        Chunk Chunk,
        ChunkEmbedding ChunkEmbedding);
}