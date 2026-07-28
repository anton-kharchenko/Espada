using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IReadOnlyList<Source>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            return await dbContext.Sources
                .AsNoTracking()
                .Where(source => source.WorkspaceId == workspaceId)
                .OrderBy(source => source.Name)
                .ThenBy(source => source.Id)
                .ToArrayAsync(cancellationToken);
        }
    }
}
