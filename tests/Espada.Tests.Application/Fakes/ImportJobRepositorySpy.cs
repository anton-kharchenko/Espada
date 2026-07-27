using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes;

internal sealed class ImportJobRepositorySpy : IImportJobRepository
{
    public ImportJob? AddedImportJob { get; private set; }

    public ImportJob? ImportJobToReturn { get; set; }
    public ImportJob? ImportJobByIdempotencyKeyToReturn { get; set; }

    public int AddCallCount { get; private set; }

    public int GetByIdCallCount { get; private set; }

    public ImportJobId? ReceivedImportJobId { get; private set; }

    public CancellationToken AddCancellationToken { get; private set; }

    public CancellationToken GetByIdCancellationToken { get; private set; }

    public Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importJob);

        AddedImportJob = importJob;
        AddCallCount++;
        AddCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public Task<ImportJob?> GetByIdAsync(ImportJobId importJobId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importJobId);

        ReceivedImportJobId = importJobId;
        GetByIdCallCount++;
        GetByIdCancellationToken = cancellationToken;

        return Task.FromResult(ImportJobToReturn);
    }

    public Task<ImportJob?> GetByIdempotencyKeyAsync(
        WorkspaceId workspaceId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ImportJobByIdempotencyKeyToReturn);
    }

    public Task<bool> IsBlobReferencedByOtherImportAsync(
        ImportJobId importJobId,
        string blobHash,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}