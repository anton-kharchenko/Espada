using Espada.Domain.Aggregates;

namespace Espada.Tests.Infrastructure.TestData
{
    internal static class RequiredForeignKeyTestData
    {
        public static TheoryData<Type, string, Type> Relationships =>
        [
            (typeof(Source), nameof(Source.WorkspaceId), typeof(Workspace)),
            (typeof(ImportJob), nameof(ImportJob.SourceId), typeof(Source)),
            (typeof(ImportJob), nameof(ImportJob.WorkspaceId), typeof(Workspace)),
            (typeof(Artifact), nameof(Artifact.WorkspaceId), typeof(Workspace)),
            (typeof(ChunkBatch), nameof(ChunkBatch.ArtifactRevisionId), typeof(ArtifactRevision)),
            (typeof(Chunk), nameof(Chunk.BatchId), typeof(ChunkBatch)),
            (typeof(Chunk), nameof(Chunk.ArtifactId), typeof(Artifact)),
            (typeof(Chunk), nameof(Chunk.ArtifactRevisionId), typeof(ArtifactRevision)),
            (typeof(ChunkEmbedding), nameof(ChunkEmbedding.ChunkId), typeof(Chunk))
        ];
    }
}