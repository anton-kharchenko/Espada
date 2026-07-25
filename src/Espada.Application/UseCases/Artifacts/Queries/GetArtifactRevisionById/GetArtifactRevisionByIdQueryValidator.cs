using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    internal sealed class GetArtifactRevisionByIdQueryValidator
        : AbstractValidator<GetArtifactRevisionByIdQuery>
    {
        public GetArtifactRevisionByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.ArtifactId)
                .NotEmpty();

            RuleFor(query => query.ArtifactRevisionId)
                .NotEmpty();
        }
    }
}