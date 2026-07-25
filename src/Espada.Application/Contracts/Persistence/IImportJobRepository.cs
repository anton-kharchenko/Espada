using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence
{
    public interface IImportJobRepository
    {
        Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default);
    }
}