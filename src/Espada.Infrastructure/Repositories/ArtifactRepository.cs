using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ArtifactRepository(EspadaDbContext dbContext) : IArtifactRepository
    {
        public async Task AddAsync(Artifact artifact, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifact);

            await dbContext.Artifacts.AddAsync(artifact, cancellationToken);
        }

        public async Task<Artifact?> GetByIdAsync(ArtifactId artifactId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactId);

            return await dbContext.Artifacts.FindAsync([artifactId], cancellationToken);
        }

        public async Task<IReadOnlyList<Artifact>> ListByWorkspaceIdAsync(WorkspaceId workspaceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            return await dbContext.Artifacts
                .AsNoTracking()
                .Where(artifact => artifact.WorkspaceId == workspaceId)
                .ToListAsync(cancellationToken);
        }
    }
}