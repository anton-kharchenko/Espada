using Espada.Application.Constants;
using FluentValidation;

namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    internal sealed class BuildContextQueryValidator : AbstractValidator<BuildContextQuery>
    {
        public BuildContextQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();

            RuleFor(query => query.ProjectId)
                .NotEqual(Guid.Empty)
                .When(query => query.ProjectId.HasValue);

            RuleFor(query => query.TaskId)
                .NotEqual(Guid.Empty)
                .When(query => query.TaskId.HasValue);

            RuleFor(query => query.Agent)
                .NotEmpty()
                .Must(ContextAgentConstants.IsSupported);

            RuleFor(query => query.TokenBudget)
                .GreaterThan(0);

            RuleFor(query => query)
                .Must(HasRequiredProject)
                .WithMessage(
                    "Project ID is required when task, path, or branch context is supplied.");
        }

        private static bool HasRequiredProject(BuildContextQuery query)
        {
            return query.ProjectId.HasValue
                   || (!query.TaskId.HasValue
                       && string.IsNullOrWhiteSpace(query.RepositoryRelativePath)
                       && string.IsNullOrWhiteSpace(query.Branch));
        }
    }
}