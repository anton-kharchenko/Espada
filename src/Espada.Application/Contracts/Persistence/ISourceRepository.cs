using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence;

public interface ISourceRepository
{
    Task AddAsync(Source source, CancellationToken cancellationToken = default);
}