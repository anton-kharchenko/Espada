using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Db.Models;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class RepositoryManifestStore(EspadaDbContext dbContext) : IRepositoryManifestStore
    {
        public async Task<IReadOnlyDictionary<string, string>> LoadHashesAsync(SourceId sourceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceId);
            return await dbContext.RepositoryManifestEntries.AsNoTracking()
                .Where(entry => entry.SourceId == sourceId.Value)
                .ToDictionaryAsync(entry => entry.RelativePath, entry => entry.ContentHash,
                    StringComparer.Ordinal, cancellationToken);
        }

        public async Task ReplaceAsync(SourceId sourceId, IReadOnlyList<RepositoryFileRecord> files,
            DateTimeOffset scannedAtUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceId);
            ArgumentNullException.ThrowIfNull(files);
            RepositoryManifestEntries[] existing = await dbContext.RepositoryManifestEntries
                .Where(entry => entry.SourceId == sourceId.Value)
                .ToArrayAsync(cancellationToken);
            Dictionary<string, RepositoryManifestEntries> existingByPath = existing
                .ToDictionary(entry => entry.RelativePath, StringComparer.Ordinal);
            HashSet<string> currentPaths = files.Select(file => file.RelativePath)
                .ToHashSet(StringComparer.Ordinal);
            dbContext.RepositoryManifestEntries.RemoveRange(
                existing.Where(entry => !currentPaths.Contains(entry.RelativePath)));

            foreach (RepositoryFileRecord file in files)
            {
                if (!existingByPath.TryGetValue(file.RelativePath, out RepositoryManifestEntries? entry))
                {
                    entry = new RepositoryManifestEntries
                    {
                        SourceId = sourceId.Value,
                        RelativePath = file.RelativePath
                    };
                    dbContext.RepositoryManifestEntries.Add(entry);
                }

                entry.ContentHash = file.ContentHash;
                entry.FileName = file.FileName;
                entry.MediaType = file.MediaType;
                entry.SizeInBytes = file.SizeInBytes;
                entry.ScannedAtUtc = scannedAtUtc;
            }
        }
    }
}