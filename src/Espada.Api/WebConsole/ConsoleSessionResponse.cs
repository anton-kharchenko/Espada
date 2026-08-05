namespace Espada.Api.WebConsole
{
    internal sealed record ConsoleSessionResponse(
        bool Authenticated,
        string Mode,
        ConsoleUserResponse? User,
        IReadOnlyList<ConsoleWorkspaceResponse> Workspaces,
        bool ReadOnly);
}