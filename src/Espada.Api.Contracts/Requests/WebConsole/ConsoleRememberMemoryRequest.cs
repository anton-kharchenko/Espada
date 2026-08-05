namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleRememberMemoryRequest(
        string Title,
        string Content,
        int CategoryTypeId,
        decimal Confidence,
        Guid? SupersededMemoryId = null);
}