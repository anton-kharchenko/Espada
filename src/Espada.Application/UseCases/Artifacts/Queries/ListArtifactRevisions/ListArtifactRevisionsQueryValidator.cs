using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    internal sealed class ListArtifactRevisionsQueryValidator
        : AbstractValidator<ListArtifactRevisionsQuery>
    {
        public ListArtifactRevisionsQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.ArtifactId)
                .NotEmpty();
        }
    }
}