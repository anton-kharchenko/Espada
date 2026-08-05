namespace Espada.Api.Contracts.Responses.WebConsole
{
    public sealed record ConsoleBootstrapLinkResponse(
        string Url,
        int ExpiresIn);
}