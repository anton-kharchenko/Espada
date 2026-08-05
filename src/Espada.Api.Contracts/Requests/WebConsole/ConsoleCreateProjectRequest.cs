namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleCreateProjectRequest(
        string Name,
        string? CanonicalRemoteUri,
        IReadOnlyList<string>? LocalAliases = null);
}