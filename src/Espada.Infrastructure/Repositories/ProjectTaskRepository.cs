using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ProjectTaskRepository(
        EspadaDbContext dbContext) : IProjectTaskRepository
    {
        public async Task AddAsync(
            ProjectTask task,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(task);
            await dbContext.Tasks.AddAsync(task, cancellationToken);
        }

        public async Task<ProjectTask?> GetByIdAsync(
            TaskId taskId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(taskId);
            return await dbContext.Tasks.FindAsync([taskId], cancellationToken);
        }

        public async Task<IReadOnlyList<ProjectTask>> ListByProjectIdAsync(
            WorkspaceId workspaceId,
            ProjectId projectId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(projectId);
            return await dbContext.Tasks
                .AsNoTracking()
                .Where(task => task.WorkspaceId == workspaceId && task.ProjectId == projectId)
                .OrderBy(task => task.Status)
                .ThenByDescending(task => task.UpdatedAtUtc)
                .ThenBy(task => task.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProjectTask>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return await dbContext.Tasks
                .AsNoTracking()
                .Where(task => task.WorkspaceId == workspaceId)
                .OrderBy(task => task.Status)
                .ThenByDescending(task => task.UpdatedAtUtc)
                .ThenBy(task => task.Id)
                .ToArrayAsync(cancellationToken);
        }
    }
}
