using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ProjectTaskRepositorySpy : IProjectTaskRepository
    {
        public ProjectTask? TaskToReturn { get; set; }
        public IReadOnlyList<ProjectTask> TasksToReturn { get; set; } = [];
        public ProjectTask? AddedTask { get; private set; }
        public CancellationToken GetCancellationToken { get; private set; }

        public Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default)
        {
            AddedTask = task;
            return Task.CompletedTask;
        }

        public Task<ProjectTask?> GetByIdAsync(TaskId taskId, CancellationToken cancellationToken = default)
        {
            GetCancellationToken = cancellationToken;
            return Task.FromResult(TaskToReturn);
        }

        public Task<IReadOnlyList<ProjectTask>> ListByProjectIdAsync(WorkspaceId workspaceId, ProjectId projectId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TasksToReturn);
        }

        public Task<IReadOnlyList<ProjectTask>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TasksToReturn);
        }
    }
}