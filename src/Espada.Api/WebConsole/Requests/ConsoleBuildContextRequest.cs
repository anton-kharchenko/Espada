namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleBuildContextRequest(
        Guid? ProjectId,
        Guid? TaskId,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        int TokenBudget);
}