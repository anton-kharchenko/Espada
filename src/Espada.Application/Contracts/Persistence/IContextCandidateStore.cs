using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IContextCandidateStore
    {
        Task<IReadOnlyList<ContextCandidateRecord>> LoadByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
    }
}