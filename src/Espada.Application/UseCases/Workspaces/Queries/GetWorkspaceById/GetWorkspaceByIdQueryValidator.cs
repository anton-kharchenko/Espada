using FluentValidation;

namespace Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById
{
    internal sealed class GetWorkspaceByIdQueryValidator : AbstractValidator<GetWorkspaceByIdQuery>
    {
        public GetWorkspaceByIdQueryValidator()
        {
            RuleFor(query => query.WorkspaceId).NotEmpty();
        }
    }
}