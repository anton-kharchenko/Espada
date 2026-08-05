using Espada.Api.Contracts.Requests.WebConsole;

namespace Espada.Api.Mappings.WebConsole
{
    internal sealed record RememberMemoryMappingSource(
        Guid WorkspaceId,
        ConsoleRememberMemoryRequest Request,
        string ClientIdentity,
        string SessionIdentity);
}