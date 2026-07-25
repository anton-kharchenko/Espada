using FluentValidation;

namespace Espada.Application.UseCases.Imports.Queries.GetImportById
{
    internal sealed class GetImportByIdQueryValidator : AbstractValidator<GetImportByIdQuery>
    {
        public GetImportByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.ImportJobId)
                .NotEmpty();
        }
    }
}