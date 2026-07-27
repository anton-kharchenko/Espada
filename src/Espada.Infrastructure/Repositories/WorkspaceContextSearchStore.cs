using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories;

internal sealed class WorkspaceContextSearchStore(WorkspaceContextSearchDbContext dbContext, IOptions<WorkspaceContextSearchOptions> options) : IWorkspaceContextSearchStore
{
    private const string TextSearchConfiguration = "simple";

    public async Task<IReadOnlyList<WorkspaceContextSearchHit>> SearchAsync(WorkspaceContextSearch search, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(search);

        Guid[] artifactIds = [.. search.ArtifactIds];
        Guid[] revisionIds = [.. search.RevisionIds];
        Guid[] sourceIds = [.. search.SourceIds];
        int[] artifactTypeIds = [.. search.ArtifactTypeIds];
        int[] sourceTypeIds = [.. search.SourceTypeIds];
        Vector queryVector = new(search.QueryVector.ToArray());

        int activeArtifactStatus = ArtifactStatusType.Active.Id;
        int activeSourceStatus = SourceStatusType.Active.Id;
        int succeededImportStatus = ImportStatusType.Succeeded.Id;

        var activeSources =
            from importJob in dbContext.ImportJobs.AsNoTracking()
            join source in dbContext.Sources.AsNoTracking()
                on importJob.SourceId equals source.SourceId
            where importJob.StatusId == succeededImportStatus &&
                  source.StatusId == activeSourceStatus
            select new
            {
                importJob.ArtifactRevisionId,
                source.SourceId,
                source.TypeId,
                source.Priority
            };

        IQueryable<SearchCandidate> query =
            from vector in dbContext.EmbeddingVectors.AsNoTracking()
            join embedding in dbContext.ChunkEmbeddings.AsNoTracking()
                on vector.ChunkEmbeddingId equals embedding.ChunkEmbeddingId
            join chunk in dbContext.Chunks.AsNoTracking()
                on embedding.ChunkId equals chunk.ChunkId
            join artifact in dbContext.Artifacts.AsNoTracking()
                on chunk.ArtifactId equals artifact.ArtifactId
            let sourcePriority = activeSources
                .Where(source => source.ArtifactRevisionId == chunk.ArtifactRevisionId)
                .Select(source => (int?)source.Priority)
                .Max() ?? 0
            let similarity = 1d - vector.Vector.CosineDistance(queryVector)
            where embedding.WorkspaceId == search.WorkspaceId &&
                  embedding.ModelIdentifier == search.ModelIdentifier &&
                  embedding.ModelVersion == search.ModelVersion &&
                  embedding.Dimensions == search.QueryVector.Count &&
                  artifact.StatusId == activeArtifactStatus &&
                  (revisionIds.Length == 0 ? artifact.CurrentRevisionId == chunk.ArtifactRevisionId : ((IEnumerable<Guid>)revisionIds).Contains(chunk.ArtifactRevisionId)) &&
                  (artifactIds.Length == 0 || ((IEnumerable<Guid>)artifactIds).Contains(chunk.ArtifactId)) &&
                  (artifactTypeIds.Length == 0 || ((IEnumerable<int>)artifactTypeIds).Contains(artifact.TypeId)) &&
                  (sourceIds.Length == 0 || activeSources.Any(source => source.ArtifactRevisionId == chunk.ArtifactRevisionId && ((IEnumerable<Guid>)sourceIds).Contains(source.SourceId))) &&
                  (sourceTypeIds.Length == 0 || activeSources.Any(source => source.ArtifactRevisionId == chunk.ArtifactRevisionId && ((IEnumerable<int>)sourceTypeIds).Contains(source.TypeId))) &&
                  (search.CreatedAfterUtc == null || chunk.CreatedAtUtc >= search.CreatedAfterUtc) &&
                  (search.MinimumSimilarity == null || similarity >= search.MinimumSimilarity) &&
                  (search.MinimumArtifactPriority == null || artifact.Priority >= search.MinimumArtifactPriority) &&
                  (search.MinimumSourcePriority == null || sourcePriority >= search.MinimumSourcePriority)
            select new SearchCandidate(
                chunk.ChunkId,
                chunk.ArtifactId,
                chunk.ArtifactRevisionId,
                chunk.Content,
                chunk.SourceSpan == null ? null : chunk.SourceSpan!.Start,
                chunk.SourceSpan == null ? null : chunk.SourceSpan!.Length,
                chunk.CreatedAtUtc,
                similarity,
                EF.Functions
                    .ToTsVector(TextSearchConfiguration, chunk.Content)
                    .RankCoverDensity(EF.Functions.WebSearchToTsQuery(TextSearchConfiguration, search.QueryText), NpgsqlTsRankingNormalization.DivideByItselfPlusOne),
                artifact.Priority,
                sourcePriority);

        SearchCandidate[] candidates = await query.ToArrayAsync(cancellationToken);
        WorkspaceContextSearchOptions ranking = options.Value;

        return
        [
            .. candidates
                .Select(candidate => Score(candidate, search.NowUtc, ranking))
                .OrderByDescending(hit => hit.Score)
                .ThenByDescending(hit => hit.Similarity)
                .ThenBy(hit => hit.ChunkId)
                .Take(search.TopK)
        ];
    }

    private static WorkspaceContextSearchHit Score(SearchCandidate candidate, DateTimeOffset nowUtc, WorkspaceContextSearchOptions options)
    {
        double vectorScore = Math.Clamp((candidate.Similarity + 1d) / 2d, 0d, 1d);
        double ageDays = Math.Max(0d, (nowUtc - candidate.CreatedAtUtc).TotalDays);
        double recencyScore = Math.Pow(0.5d, ageDays / options.RecencyHalfLifeDays);
        double artifactPriorityScore = candidate.ArtifactPriority / (double)ContextPriority.Maximum;
        double sourcePriorityScore = candidate.SourcePriority / (double)ContextPriority.Maximum;
        double score = Math.Clamp(
            options.VectorWeight * vectorScore +
            options.KeywordWeight * candidate.KeywordScore +
            options.RecencyWeight * recencyScore +
            options.ArtifactPriorityWeight * artifactPriorityScore +
            options.SourcePriorityWeight * sourcePriorityScore,
            0d,
            1d);

        return new WorkspaceContextSearchHit(
            candidate.ChunkId,
            candidate.ArtifactId,
            candidate.RevisionId,
            candidate.Content,
            candidate.SourceSpanStart,
            candidate.SourceSpanLength,
            candidate.Similarity,
            candidate.KeywordScore,
            recencyScore,
            artifactPriorityScore,
            sourcePriorityScore,
            score);
    }

    private sealed record SearchCandidate(
        Guid ChunkId,
        Guid ArtifactId,
        Guid RevisionId,
        string Content,
        int? SourceSpanStart,
        int? SourceSpanLength,
        DateTimeOffset CreatedAtUtc,
        double Similarity,
        double KeywordScore,
        int ArtifactPriority,
        int SourcePriority);
}