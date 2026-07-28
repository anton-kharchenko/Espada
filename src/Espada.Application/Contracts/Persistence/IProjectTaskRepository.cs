using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IProjectTaskRepository
    {
        Task AddAsync(
            ProjectTask task,
            CancellationToken cancellationToken = default);

        Task<ProjectTask?> GetByIdAsync(
            TaskId taskId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProjectTask>> ListByProjectIdAsync(
            WorkspaceId workspaceId,
            ProjectId projectId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProjectTask>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
    }
}
