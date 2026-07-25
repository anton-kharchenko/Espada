using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ArtifactRevisionRepository(EspadaDbContext dbContext) : IArtifactRevisionRepository
    {
        public async Task AddAsync(ArtifactRevision artifactRevision, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevision);

            await dbContext.ArtifactRevisions.AddAsync(artifactRevision, cancellationToken);
        }

        public async Task<ArtifactRevision?> GetByIdAsync(ArtifactRevisionId artifactRevisionId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevisionId);

            return await dbContext.ArtifactRevisions
                .AsNoTracking()
                .SingleOrDefaultAsync(revision => revision.Id == artifactRevisionId, cancellationToken);
        }

        public async Task<IReadOnlyList<ArtifactRevision>> ListByArtifactIdAsync(ArtifactId artifactId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactId);

            return await dbContext.ArtifactRevisions
                .AsNoTracking()
                .Where(revision => revision.ArtifactId == artifactId)
                .OrderByDescending(revision => revision.Number)
                .ToListAsync(cancellationToken);
        }
    }
}