using Espada.Application.Models.Agents;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Agents
{
    public interface IAgentWorktreeService
    {
        Task<DomainResult<AgentWorktree>> PrepareAsync(Project project, AgentSessionId sessionId,
            AgentVendorType vendor, CancellationToken cancellationToken = default);
        Task<DomainResult> ApplyAsync(Project project, AgentWorktree worktree,
            CancellationToken cancellationToken = default);
        Task<DomainResult> RemoveAsync(Project project, AgentWorktree worktree,
            CancellationToken cancellationToken = default);
    }
}