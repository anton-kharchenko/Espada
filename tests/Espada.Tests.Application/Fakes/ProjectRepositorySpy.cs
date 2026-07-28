using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ProjectRepositorySpy : IProjectRepository
    {
        public Project? ProjectToReturn { get; set; }
        public IReadOnlyList<Project> ProjectsToReturn { get; set; } = [];
        public bool CanonicalRemoteExists { get; set; }
        public Project? AddedProject { get; private set; }
        public CancellationToken GetCancellationToken { get; private set; }

        public Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            AddedProject = project;
            return Task.CompletedTask;
        }

        public Task<Project?> GetByIdAsync(ProjectId projectId, CancellationToken cancellationToken = default)
        {
            GetCancellationToken = cancellationToken;
            return Task.FromResult(ProjectToReturn);
        }

        public Task<IReadOnlyList<Project>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ProjectsToReturn);
        }

        public Task<bool> ExistsByCanonicalRemoteUriAsync(WorkspaceId workspaceId, string canonicalRemoteUri,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CanonicalRemoteExists);
        }
    }
}