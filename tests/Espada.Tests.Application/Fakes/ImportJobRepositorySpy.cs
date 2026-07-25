using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ImportJobRepositorySpy : IImportJobRepository
    {
        public ImportJob? AddedImportJob { get; private set; }

        public int AddCallCount { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJob);

            AddedImportJob = importJob;
            AddCallCount++;
            AddCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}