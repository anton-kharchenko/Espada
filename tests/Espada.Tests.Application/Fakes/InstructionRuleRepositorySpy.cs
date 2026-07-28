using Espada.Application.Contracts.Persistence;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class InstructionRuleRepositorySpy : IInstructionRuleRepository
    {
        public IReadOnlyList<InstructionRule> AddedRules { get; private set; } = [];
        public IReadOnlyList<InstructionRule> RulesToReturn { get; set; } = [];
        public CancellationToken AddCancellationToken { get; private set; }
        public CancellationToken ListCancellationToken { get; private set; }

        public Task AddRangeAsync(
            IReadOnlyList<InstructionRule> rules,
            CancellationToken cancellationToken = default)
        {
            AddedRules = rules;
            AddCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<InstructionRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default)
        {
            ListCancellationToken = cancellationToken;
            return Task.FromResult(RulesToReturn);
        }
    }
}