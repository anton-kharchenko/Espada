using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;

internal sealed class SearchWorkspaceContextQueryValidator : AbstractValidator<SearchWorkspaceContextQuery>
{
    public SearchWorkspaceContextQueryValidator()
    {
        RuleFor(query => query.WorkspaceId).NotEmpty();
        RuleFor(query => query.QueryText).NotEmpty();
        RuleFor(query => query.QueryVector).NotNull().NotEmpty();
        RuleForEach(query => query.QueryVector).Must(float.IsFinite);
        RuleFor(query => query.ModelIdentifier).NotEmpty();
        RuleFor(query => query.ModelVersion).NotEmpty();
        RuleFor(query => query.TopK).InclusiveBetween(1, 100);
        RuleFor(query => query.MinimumSimilarity).InclusiveBetween(-1, 1).When(query => query.MinimumSimilarity.HasValue);
        RuleFor(query => query.MinimumArtifactPriority).InclusiveBetween(ContextPriority.Minimum, ContextPriority.Maximum).When(query => query.MinimumArtifactPriority.HasValue);
        RuleFor(query => query.MinimumSourcePriority).InclusiveBetween(ContextPriority.Minimum, ContextPriority.Maximum).When(query => query.MinimumSourcePriority.HasValue);
    }
}