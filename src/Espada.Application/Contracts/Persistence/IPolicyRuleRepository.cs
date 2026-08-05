using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IPolicyRuleRepository
    {
        Task AddRangeAsync(
            IReadOnlyList<PolicyRule> rules,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PolicyRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default);
    }
}