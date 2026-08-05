using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    public sealed record ContextResolutionRequest(
        Workspace Workspace,
        Project? Project,
        ProjectTask? Task,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        int TokenBudget);
}