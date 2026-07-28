using Espada.Application.Contracts.Persistence;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class InstructionRuleRepository(
        EspadaDbContext dbContext) : IInstructionRuleRepository
    {
        public async Task AddRangeAsync(
            IReadOnlyList<InstructionRule> rules,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(rules);
            await dbContext.InstructionRules.AddRangeAsync(rules, cancellationToken);
        }

        public async Task<IReadOnlyList<InstructionRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(revisionId);
            return await dbContext.InstructionRules
                .AsNoTracking()
                .Where(rule => rule.ArtifactRevisionId == revisionId)
                .OrderByDescending(rule => rule.Priority)
                .ThenBy(rule => rule.RuleKey)
                .ToListAsync(cancellationToken);
        }
    }
}