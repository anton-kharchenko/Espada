namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleCreateProjectTaskRequest(
        Guid ProjectId,
        string Title);
}