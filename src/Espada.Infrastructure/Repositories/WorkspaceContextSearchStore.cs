using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class WorkspaceContextSearchStore(
        WorkspaceContextSearchDbContext dbContext,
        IOptions<WorkspaceContextSearchOptions> options)
        : IWorkspaceContextSearchStore
    {
        private const string TextSearchConfiguration = "simple";

        public async Task<IReadOnlyList<WorkspaceContextSearchHit>> SearchAsync(
            WorkspaceContextSearch search,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(search);

            Guid[] artifactIds = [.. search.ArtifactIds];
            Guid[] revisionIds = [.. search.RevisionIds];
            Guid[] sourceIds = [.. search.SourceIds];
            int[] artifactTypeIds = [.. search.ArtifactTypeIds];
            string[] artifactKinds = [.. search.ArtifactKinds];
            int[] sourceTypeIds = [.. search.SourceTypeIds];
            string[] memoryCategories = [.. search.MemoryCategories];

            int activeArtifactStatus = ArtifactStatusType.Active.Id;
            int activeSourceStatus = SourceStatusType.Active.Id;
            int succeededImportStatus = ImportStatusType.Succeeded.Id;
            string memoryKind = ArtifactKindType.Memory.Name;

            IQueryable<Guid?> activeSourceRevisionIds =
                from importJob in dbContext.ImportJobs.AsNoTracking()
                join source in dbContext.Sources.AsNoTracking()
                    on importJob.SourceId equals source.SourceId
                where importJob.StatusId == succeededImportStatus &&
                      source.StatusId == activeSourceStatus
                select importJob.ArtifactRevisionId;
            IQueryable<Guid?> selectedSourceRevisionIds =
                from importJob in dbContext.ImportJobs.AsNoTracking()
                join source in dbContext.Sources.AsNoTracking()
                    on importJob.SourceId equals source.SourceId
                where importJob.StatusId == succeededImportStatus &&
                      source.StatusId == activeSourceStatus &&
                      (sourceIds.Length == 0 || sourceIds.Contains(source.SourceId)) &&
                      (sourceTypeIds.Length == 0 || sourceTypeIds.Contains(source.TypeId))
                select importJob.ArtifactRevisionId;
            IQueryable<MemoryMetadataRecords> currentMemories =
                dbContext.MemoryMetadata
                    .AsNoTracking()
                    .Where(metadata =>
                        !dbContext.MemoryMetadata.Any(newer => newer.SupersededMemoryId == metadata.MemoryId));
            bool hasQueryVector = search.QueryVector.Count > 0;
            IQueryable<SearchCandidate> query =
                from chunk in dbContext.Chunks.AsNoTracking()
                join artifact in dbContext.Artifacts.AsNoTracking()
                    on chunk.ArtifactId equals artifact.ArtifactId
                let sourcePriority = dbContext.ImportJobs
                    .Where(importJob =>
                        importJob.StatusId == succeededImportStatus &&
                        importJob.ArtifactRevisionId == chunk.ArtifactRevisionId)
                    .Join(
                        dbContext.Sources.Where(source =>
                            source.StatusId == activeSourceStatus),
                        importJob => importJob.SourceId,
                        source => source.SourceId,
                        (_, source) => (int?)source.Priority)
                    .Max() ?? 0
                where artifact.WorkspaceId == search.WorkspaceId
                where artifact.StatusId == activeArtifactStatus
                where activeSourceRevisionIds.Contains(chunk.ArtifactRevisionId) ||
                      (artifact.Kind == memoryKind && currentMemories.Any(metadata =>
                          metadata.ArtifactId == chunk.ArtifactId &&
                          metadata.ArtifactRevisionId == chunk.ArtifactRevisionId))
                where revisionIds.Length == 0
                    ? artifact.CurrentRevisionId == chunk.ArtifactRevisionId
                    : revisionIds.Contains(chunk.ArtifactRevisionId)
                where artifactIds.Length == 0 || artifactIds.Contains(chunk.ArtifactId)
                where artifactTypeIds.Length == 0 || artifactTypeIds.Contains(artifact.TypeId)
                where artifactKinds.Length == 0 || artifactKinds.Contains(artifact.Kind)
                where (sourceIds.Length == 0 && sourceTypeIds.Length == 0) ||
                      selectedSourceRevisionIds.Contains(chunk.ArtifactRevisionId)
                where memoryCategories.Length == 0 || currentMemories.Any(metadata =>
                    metadata.ArtifactId == chunk.ArtifactId &&
                    metadata.ArtifactRevisionId == chunk.ArtifactRevisionId &&
                    memoryCategories.Contains(metadata.Category))
                where search.CreatedAfterUtc == null || chunk.CreatedAtUtc >= search.CreatedAfterUtc
                where search.MinimumArtifactPriority == null || artifact.Priority >= search.MinimumArtifactPriority
                where search.MinimumSourcePriority == null || sourcePriority >= search.MinimumSourcePriority
                select new SearchCandidate(
                    chunk.ChunkId,
                    chunk.ArtifactId,
                    chunk.ArtifactRevisionId,
                    chunk.Content,
                    chunk.SourceSpan == null ? null : chunk.SourceSpan!.Start,
                    chunk.SourceSpan == null ? null : chunk.SourceSpan!.Length,
                    chunk.CreatedAtUtc,
                    EF.Functions
                        .ToTsVector(TextSearchConfiguration, chunk.Content)
                        .RankCoverDensity(
                            EF.Functions.WebSearchToTsQuery(
                                TextSearchConfiguration,
                                search.QueryText),
                            NpgsqlTsRankingNormalization.DivideByItselfPlusOne),
                    artifact.Priority,
                    sourcePriority);

            SearchCandidate[] candidates = await query.ToArrayAsync(cancellationToken);
            Dictionary<Guid, double> similaritiesByChunkId = [];
            if (hasQueryVector && candidates.Length > 0)
            {
                Vector queryVector = new(search.QueryVector.ToArray());
                Guid[] candidateChunkIds = candidates
                    .Select(candidate => candidate.ChunkId)
                    .ToArray();
                IQueryable<SearchVectorCandidate> similarityQuery =
                    from vector in dbContext.EmbeddingVectors.AsNoTracking()
                    join embedding in dbContext.ChunkEmbeddings.AsNoTracking()
                        on vector.ChunkEmbeddingId equals embedding.ChunkEmbeddingId
                    where embedding.WorkspaceId == search.WorkspaceId
                    where embedding.ModelIdentifier == search.ModelIdentifier
                    where embedding.ModelVersion == search.ModelVersion
                    where embedding.Dimensions == search.QueryVector.Count
                    where candidateChunkIds.Contains(embedding.ChunkId)
                    select new SearchVectorCandidate(
                        embedding.ChunkId,
                        1d - vector.Vector.CosineDistance(queryVector));

                SearchVectorCandidate[] similarities =
                    await similarityQuery.ToArrayAsync(cancellationToken);
                similaritiesByChunkId = similarities
                    .GroupBy(candidate => candidate.ChunkId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Max(candidate => candidate.Similarity));
            }

            WorkspaceContextSearchOptions ranking = options.Value;

            IEnumerable<WorkspaceContextSearchHit> rankedHits = candidates
                .Where(candidate => !hasQueryVector ||
                                    similaritiesByChunkId.ContainsKey(candidate.ChunkId) ||
                                    candidate.KeywordScore > 0d)
                .Where(candidate => search.MinimumSimilarity == null ||
                                    (similaritiesByChunkId.TryGetValue(candidate.ChunkId, out double similarity) &&
                                     similarity >= search.MinimumSimilarity))
                .Select(candidate => Score(
                    candidate,
                    search.NowUtc,
                    ranking,
                    similaritiesByChunkId.TryGetValue(candidate.ChunkId, out double similarity)
                        ? similarity
                        : null))
                .Where(hit => hasQueryVector || hit.KeywordScore > 0d);

            if (search.DistinctRevisions)
            {
                rankedHits = rankedHits
                    .GroupBy(hit => hit.RevisionId)
                    .Select(group => group
                        .OrderByDescending(hit => hit.Score)
                        .ThenByDescending(hit => hit.Similarity)
                        .ThenBy(hit => hit.ChunkId)
                        .First());
            }

            return
            [
                .. rankedHits
                    .OrderByDescending(hit => hit.Score)
                    .ThenByDescending(hit => hit.Similarity)
                    .ThenBy(hit => hit.ChunkId)
                    .Take(search.TopK)
            ];
        }

        private static WorkspaceContextSearchHit Score(
            SearchCandidate candidate,
            DateTimeOffset nowUtc,
            WorkspaceContextSearchOptions options,
            double? similarity)
        {
            double vectorScore = similarity.HasValue
                ? Math.Clamp((similarity.Value + 1d) / 2d, 0d, 1d)
                : 0d;
            double ageDays = Math.Max(0d, (nowUtc - candidate.CreatedAtUtc).TotalDays);
            double recencyScore = Math.Pow(0.5d, ageDays / options.RecencyHalfLifeDays);
            double artifactPriorityScore =
                candidate.ArtifactPriority / (double)ContextPriority.Maximum;
            double sourcePriorityScore =
                candidate.SourcePriority / (double)ContextPriority.Maximum;
            double score = Math.Clamp(
                (options.VectorWeight * vectorScore) +
                (options.KeywordWeight * candidate.KeywordScore) +
                (options.RecencyWeight * recencyScore) +
                (options.ArtifactPriorityWeight * artifactPriorityScore) +
                (options.SourcePriorityWeight * sourcePriorityScore),
                0d,
                1d);

            return new WorkspaceContextSearchHit(
                candidate.ChunkId,
                candidate.ArtifactId,
                candidate.RevisionId,
                candidate.Content,
                candidate.SourceSpanStart,
                candidate.SourceSpanLength,
                similarity ?? 0d,
                candidate.KeywordScore,
                recencyScore,
                artifactPriorityScore,
                sourcePriorityScore,
                score);
        }
    }
}