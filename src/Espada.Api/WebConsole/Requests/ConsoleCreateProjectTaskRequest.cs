namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleCreateProjectTaskRequest(
        Guid ProjectId,
        string Title);
}
