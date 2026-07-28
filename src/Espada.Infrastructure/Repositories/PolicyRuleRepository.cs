using Espada.Application.Contracts.Persistence;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class PolicyRuleRepository(
        EspadaDbContext dbContext) : IPolicyRuleRepository
    {
        public async Task AddRangeAsync(
            IReadOnlyList<PolicyRule> rules,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(rules);
            await dbContext.PolicyRules.AddRangeAsync(rules, cancellationToken);
        }

        public async Task<IReadOnlyList<PolicyRule>> ListByRevisionIdAsync(
            ArtifactRevisionId revisionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(revisionId);
            return await dbContext.PolicyRules
                .AsNoTracking()
                .Where(rule => rule.ArtifactRevisionId == revisionId)
                .OrderByDescending(rule => rule.Priority)
                .ThenBy(rule => rule.RuleKey)
                .ToListAsync(cancellationToken);
        }
    }
}