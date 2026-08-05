using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IInstructionRuleRepository
    {
        Task AddRangeAsync(
            IReadOnlyList<InstructionRule> rules,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InstructionRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default);
    }
}