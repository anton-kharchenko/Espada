using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ContextCandidateStoreSpy : IContextCandidateStore
    {
        public IReadOnlyList<ContextCandidateRecord> CandidatesToReturn { get; set; } =
            [];

        public WorkspaceId? ReceivedWorkspaceId { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<IReadOnlyList<ContextCandidateRecord>>
            LoadByWorkspaceIdAsync(
                WorkspaceId workspaceId,
                CancellationToken cancellationToken = default)
        {
            ReceivedWorkspaceId = workspaceId;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(CandidatesToReturn);
        }
    }
}