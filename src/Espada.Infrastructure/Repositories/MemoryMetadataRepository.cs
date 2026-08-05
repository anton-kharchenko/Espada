using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class MemoryMetadataRepository(
        EspadaDbContext dbContext) : IMemoryMetadataRepository
    {
        public async Task AddAsync(
            MemoryMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(metadata);
            await dbContext.MemoryMetadata.AddAsync(metadata, cancellationToken);
        }

        public async Task<MemoryMetadata?> GetByIdAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(memoryId);
            return await dbContext.MemoryMetadata
                .AsNoTracking()
                .SingleOrDefaultAsync(metadata => metadata.Id == memoryId, cancellationToken);
        }

        public async Task<bool> IsSupersededAsync(
            MemoryId memoryId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(memoryId);
            return await dbContext.MemoryMetadata
                .AsNoTracking()
                .AnyAsync(metadata => metadata.SupersededMemoryId == memoryId, cancellationToken);
        }
    }
}