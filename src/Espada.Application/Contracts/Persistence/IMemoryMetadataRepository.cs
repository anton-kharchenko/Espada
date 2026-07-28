using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IMemoryMetadataRepository
    {
        Task AddAsync(
            MemoryMetadata metadata,
            CancellationToken cancellationToken = default);

        Task<MemoryMetadata?> GetByIdAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default);

        Task<bool> IsSupersededAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default);
    }
}