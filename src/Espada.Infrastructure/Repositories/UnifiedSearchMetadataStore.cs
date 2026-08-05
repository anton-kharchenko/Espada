using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class UnifiedSearchMetadataStore(WorkspaceContextSearchDbContext dbContext)
        : IUnifiedSearchMetadataStore
    {
        public async Task<IReadOnlyList<UnifiedSearchRecord>> LoadAsync(WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(hits);
            Guid[] artifactIds = hits.Select(hit => hit.ArtifactId).Distinct().ToArray();
            Guid[] revisionIds = hits.Select(hit => hit.RevisionId).Distinct().ToArray();
            Dictionary<Guid, Artifacts> artifacts = await dbContext.Artifacts.AsNoTracking()
                .Where(artifact => artifact.WorkspaceId == workspaceId.Value && artifactIds.Contains(artifact.ArtifactId))
                .ToDictionaryAsync(artifact => artifact.ArtifactId, cancellationToken);
            ImportJobs[] imports = await dbContext.ImportJobs.AsNoTracking()
                .Where(importJob => importJob.WorkspaceId == workspaceId.Value
                                    && importJob.StatusId == ImportStatusType.Succeeded.Id
                                    && importJob.ArtifactRevisionId.HasValue
                                    && revisionIds.Contains(importJob.ArtifactRevisionId.Value))
                .OrderByDescending(importJob => importJob.CompletedAtUtc)
                .ToArrayAsync(cancellationToken);
            Guid[] sourceIds = imports.Select(importJob => importJob.SourceId).Distinct().ToArray();
            Dictionary<Guid, Sources> sources = await dbContext.Sources.AsNoTracking()
                .Where(source => source.WorkspaceId == workspaceId.Value && sourceIds.Contains(source.SourceId))
                .ToDictionaryAsync(source => source.SourceId, cancellationToken);
            HashSet<Guid> memoryRevisions = (await dbContext.MemoryMetadata.AsNoTracking()
                    .Where(memory => revisionIds.Contains(memory.ArtifactRevisionId))
                    .Select(memory => memory.ArtifactRevisionId)
                    .ToArrayAsync(cancellationToken))
                .ToHashSet();
            Dictionary<Guid, ImportJobs> importsByRevision = imports
                .Where(importJob => importJob.ArtifactRevisionId.HasValue)
                .GroupBy(importJob => importJob.ArtifactRevisionId!.Value)
                .ToDictionary(group => group.Key, group => group.First());

            List<UnifiedSearchRecord> records = [];
            foreach (WorkspaceContextSearchHit hit in hits)
            {
                if (!artifacts.TryGetValue(hit.ArtifactId, out Artifacts? artifact))
                {
                    continue;
                }

                importsByRevision.TryGetValue(hit.RevisionId, out ImportJobs? import);
                Sources? source = import is not null && sources.TryGetValue(import.SourceId, out Sources? found)
                    ? found
                    : null;
                string hitType = memoryRevisions.Contains(hit.RevisionId)
                    ? "memory"
                    : source is not null ? "source" : "artifact";
                records.Add(new UnifiedSearchRecord(hitType, hit.ChunkId, hit.ArtifactId, hit.RevisionId,
                    source?.SourceId, source?.TypeId, artifact.Kind, artifact.TypeId, artifact.Title, hit.Content,
                    hit.SourceSpanStart, hit.SourceSpanLength, hit.Score,
                    source is null ? $"artifact:{artifact.ArtifactId:D}" : $"source:{source.SourceId:D}:{source.Locator}"));
            }

            return records;
        }
    }
}