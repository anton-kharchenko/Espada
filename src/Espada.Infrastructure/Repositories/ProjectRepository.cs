using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ProjectRepository(
        EspadaDbContext dbContext) : IProjectRepository
    {
        public async Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);
            await dbContext.Projects.AddAsync(project, cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(
            ProjectId projectId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(projectId);
            return await dbContext.Projects.FindAsync([projectId], cancellationToken);
        }

        public async Task<IReadOnlyList<Project>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return await dbContext.Projects
                .AsNoTracking()
                .Where(project => project.WorkspaceId == workspaceId)
                .OrderBy(project => project.Name)
                .ThenBy(project => project.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByCanonicalRemoteUriAsync(
            WorkspaceId workspaceId,
            string canonicalRemoteUri,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentException.ThrowIfNullOrWhiteSpace(canonicalRemoteUri);
            return await dbContext.Projects
                .AsNoTracking()
                .AnyAsync(
                    project => project.WorkspaceId == workspaceId
                               && project.CanonicalRemoteUri == canonicalRemoteUri,
                    cancellationToken);
        }
    }
}