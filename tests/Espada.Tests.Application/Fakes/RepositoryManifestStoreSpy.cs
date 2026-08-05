using Espada.Application.Contracts.Repositories;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class RepositoryManifestStoreSpy : IRepositoryManifestStore
    {
        public IReadOnlyDictionary<string, string> HashesToReturn { get; set; } =
            new Dictionary<string, string>();
        public IReadOnlyList<RepositoryFileRecord>? ReplacedFiles { get; private set; }

        public Task<IReadOnlyDictionary<string, string>> LoadHashesAsync(SourceId sourceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HashesToReturn);
        }

        public Task ReplaceAsync(SourceId sourceId, IReadOnlyList<RepositoryFileRecord> files,
            DateTimeOffset scannedAtUtc, CancellationToken cancellationToken = default)
        {
            ReplacedFiles = files;
            return Task.CompletedTask;
        }
    }
}