using FluentValidation;

namespace Espada.Application.UseCases.Sources.Queries.GetSourceById
{
    internal sealed class GetSourceByIdQueryValidator : AbstractValidator<GetSourceByIdQuery>
    {
        public GetSourceByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.SourceId)
                .NotEmpty();
        }
    }
}