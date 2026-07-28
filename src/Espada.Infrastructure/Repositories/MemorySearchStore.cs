using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class MemorySearchStore(
        EspadaDbContext dbContext) : IMemorySearchStore
    {
        public async Task<IReadOnlyList<MemorySearchRecord>> LoadAsync(
            WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits,
            IReadOnlyCollection<MemoryCategoryType> categoryTypes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(hits);
            ArgumentNullException.ThrowIfNull(categoryTypes);

            ArtifactRevisionId[] revisionIds = hits
                .Select(hit => ArtifactRevisionId.Create(hit.RevisionId))
                .ToArray();
            if (revisionIds.Length == 0)
            {
                return [];
            }

            IQueryable<MemorySearchRecord> query =
                from metadata in dbContext.MemoryMetadata.AsNoTracking()
                join artifact in dbContext.Artifacts.AsNoTracking()
                    on metadata.ArtifactId equals artifact.Id
                join revision in dbContext.ArtifactRevisions.AsNoTracking()
                    on metadata.ArtifactRevisionId equals revision.Id
                where artifact.WorkspaceId == workspaceId
                      && artifact.Status == ArtifactStatusType.Active
                      && artifact.KindType == ArtifactKindType.Memory
                      && artifact.CurrentRevisionId == revision.Id
                      && revisionIds.Contains(revision.Id)
                      && !dbContext.MemoryMetadata.Any(newer => newer.SupersededMemoryId == metadata.Id)
                select new MemorySearchRecord(artifact, revision, metadata, 0);

            List<MemorySearchRecord> records = await query.ToListAsync(cancellationToken);
            Dictionary<Guid, MemorySearchRecord> recordsByRevisionId = records
                .Where(record => categoryTypes.Count == 0
                                 || categoryTypes.Any(categoryType =>
                                     categoryType.Equals(record.Metadata.CategoryType)))
                .ToDictionary(record => record.Revision.Id.Value);

            return hits
                .Where(hit => recordsByRevisionId.ContainsKey(hit.RevisionId))
                .GroupBy(hit => hit.RevisionId)
                .Select(group => group
                    .OrderByDescending(hit => hit.Score)
                    .ThenByDescending(hit => hit.Similarity)
                    .ThenBy(hit => hit.ChunkId)
                    .First())
                .OrderByDescending(hit => hit.Score)
                .ThenBy(hit => hit.RevisionId)
                .Select(hit => recordsByRevisionId[hit.RevisionId] with { Score = hit.Score })
                .ToArray();
        }
    }
}