using Espada.Application.Contracts.Persistence;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class PolicyRuleRepositorySpy : IPolicyRuleRepository
    {
        public IReadOnlyList<PolicyRule> AddedRules { get; private set; } = [];
        public IReadOnlyList<PolicyRule> RulesToReturn { get; set; } = [];
        public CancellationToken AddCancellationToken { get; private set; }
        public CancellationToken ListCancellationToken { get; private set; }

        public Task AddRangeAsync(
            IReadOnlyList<PolicyRule> rules,
            CancellationToken cancellationToken = default)
        {
            AddedRules = rules;
            AddCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PolicyRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default)
        {
            ListCancellationToken = cancellationToken;
            return Task.FromResult(RulesToReturn);
        }
    }
}