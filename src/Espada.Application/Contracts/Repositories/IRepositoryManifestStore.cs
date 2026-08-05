using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Repositories
{
    public interface IRepositoryManifestStore
    {
        Task<IReadOnlyDictionary<string, string>> LoadHashesAsync(
            SourceId sourceId,
            CancellationToken cancellationToken = default);

        Task ReplaceAsync(
            SourceId sourceId,
            IReadOnlyList<RepositoryFileRecord> files,
            DateTimeOffset scannedAtUtc,
            CancellationToken cancellationToken = default);
    }
}