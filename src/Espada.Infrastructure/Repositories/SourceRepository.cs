using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class SourceRepository(EspadaDbContext dbContext) : ISourceRepository
    {
        public async Task AddAsync(Source source, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            await dbContext.Sources.AddAsync(source, cancellationToken);
        }

        public async Task<Source?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceId);

            return await dbContext.Sources.FindAsync([sourceId], cancellationToken);
        }
    }
}