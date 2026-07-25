using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    internal sealed class ListArtifactsQueryValidator
        : AbstractValidator<ListArtifactsQuery>
    {
        public ListArtifactsQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();
        }
    }
}