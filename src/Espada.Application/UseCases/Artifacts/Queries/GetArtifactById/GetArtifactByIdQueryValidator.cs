using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    internal sealed class GetArtifactByIdQueryValidator
        : AbstractValidator<GetArtifactByIdQuery>
    {
        public GetArtifactByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.ArtifactId)
                .NotEmpty();
        }
    }
}