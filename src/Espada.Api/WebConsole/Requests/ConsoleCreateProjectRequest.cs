namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleCreateProjectRequest(
        string Name,
        string CanonicalRemoteUri,
        IReadOnlyList<string>? LocalAliases = null);
}
