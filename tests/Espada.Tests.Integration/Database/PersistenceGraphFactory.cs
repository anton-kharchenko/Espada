using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Integration.Database
{
    internal static class PersistenceGraphFactory
    {
        public static PersistenceGraph Create()
        {
            DateTimeOffset createdAtUtc = new(2026, 7, 26, 5, 0, 0, TimeSpan.Zero);
            DateTimeOffset updatedAtUtc = createdAtUtc.AddMinutes(1);
            DateTimeOffset completedAtUtc = createdAtUtc.AddMinutes(2);

            Workspace workspace = Workspace.Create(WorkspaceId.New(),
                WorkspaceName.Create("Integration workspace").ShouldSucceed(), WorkspaceType.Personal, null,
                createdAtUtc).ShouldSucceed();
            Source source = Source.Create(SourceId.Create(Guid.NewGuid()), workspace.Id,
                    SourceName.Create("Integration source").ShouldSucceed(), SourceType.WebPage,
                    SourceLocator.Create($"https://example.com/{Guid.NewGuid():N}").ShouldSucceed(), createdAtUtc)
                .ShouldSucceed();
            Artifact artifact = Artifact.Create(ArtifactId.Create(Guid.NewGuid()), workspace.Id,
                ArtifactTitle.Create("Integration artifact").ShouldSucceed(), ArtifactKindType.Document,
                ArtifactType.Markdown, createdAtUtc).ShouldSucceed();
            ArtifactRevision revision = artifact.CreateRevision(ArtifactRevisionId.Create(Guid.NewGuid()),
                ArtifactContent.Create("Integration artifact content.").ShouldSucceed(), updatedAtUtc).ShouldSucceed();
            ImportJob importJob = ImportJob
                .Request(ImportJobId.Create(Guid.NewGuid()), source.Id, workspace.Id, createdAtUtc).ShouldSucceed();
            importJob.Start(updatedAtUtc).ShouldSucceed();
            importJob.Complete(artifact.Id, revision.Id, completedAtUtc).ShouldSucceed();

            ChunkBatch chunkBatch = ChunkBatch.Request(ChunkBatchId.Create(Guid.NewGuid()), workspace.Id, artifact.Id,
                revision.Id, ChunkingStrategyType.Recursive, ChunkingVersion.Create("recursive-v1").ShouldSucceed(),
                createdAtUtc).ShouldSucceed();
            chunkBatch.Start(updatedAtUtc).ShouldSucceed();
            chunkBatch.Complete(1, completedAtUtc).ShouldSucceed();

            Chunk chunk = Chunk.Create(ChunkId.Create(Guid.NewGuid()), chunkBatch.Id, workspace.Id, artifact.Id,
                revision.Id, ChunkNumber.First(), ChunkContent.Create("Integration chunk content.").ShouldSucceed(),
                SourceTextSpan.Create(0, 26).ShouldSucceed(), ChunkingStrategyType.Recursive,
                ChunkingVersion.Create("recursive-v1").ShouldSucceed(), completedAtUtc).ShouldSucceed();
            ChunkEmbedding chunkEmbedding = ChunkEmbedding.Create(ChunkEmbeddingId.Create(Guid.NewGuid()), workspace.Id,
                chunk.Id, chunk.ContentHash, EmbeddingModel.Create("test-embedding-model", "1").ShouldSucceed(),
                EmbeddingDimensions.Create(3).ShouldSucceed(), completedAtUtc).ShouldSucceed();

            return new PersistenceGraph(workspace, source, importJob, artifact, revision, chunkBatch, chunk,
                chunkEmbedding);
        }
    }
}