using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IProjectRepository
    {
        Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default);

        Task<Project?> GetByIdAsync(
            ProjectId projectId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Project>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByCanonicalRemoteUriAsync(
            WorkspaceId workspaceId,
            string canonicalRemoteUri,
            CancellationToken cancellationToken = default);
    }
}