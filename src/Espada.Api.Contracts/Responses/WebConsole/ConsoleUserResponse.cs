namespace Espada.Api.Contracts.Responses.WebConsole
{
    public sealed record ConsoleUserResponse(
        string DisplayName,
        string? Email);
}