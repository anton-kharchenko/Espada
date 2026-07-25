using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IImportJobRepository
    {
        Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default);

        Task<ImportJob?> GetByIdAsync(ImportJobId importJobId, CancellationToken cancellationToken = default);
    }
}