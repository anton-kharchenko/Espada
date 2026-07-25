using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ImportJobRepository(EspadaDbContext dbContext) : IImportJobRepository
    {
        public async Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJob);

            await dbContext.ImportJobs.AddAsync(importJob, cancellationToken);
        }

        public async Task<ImportJob?> GetByIdAsync(ImportJobId importJobId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJobId);

            return await dbContext.ImportJobs.FindAsync([importJobId], cancellationToken);
        }
    }
}