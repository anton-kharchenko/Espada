namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleBuildContextRequest(
        Guid? ProjectId,
        Guid? TaskId,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        int TokenBudget);
}