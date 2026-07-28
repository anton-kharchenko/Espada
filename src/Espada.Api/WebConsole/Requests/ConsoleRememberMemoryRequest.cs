namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleRememberMemoryRequest(
        string Title,
        string Content,
        int CategoryTypeId,
        decimal Confidence,
        Guid? SupersededMemoryId = null);
}
