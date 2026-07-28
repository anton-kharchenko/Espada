using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IImportJobRepository
    {
        Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default);

        Task<ImportJob?> GetByIdAsync(ImportJobId importJobId, CancellationToken cancellationToken = default);

        Task<ImportJob?> GetByIdempotencyKeyAsync(
            WorkspaceId workspaceId,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ImportJob>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);

        Task<bool> IsBlobReferencedByOtherImportAsync(
            ImportJobId importJobId,
            string blobHash,
            CancellationToken cancellationToken = default);
    }
}
