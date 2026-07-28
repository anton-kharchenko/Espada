using Espada.Api.WebConsole.Requests;

namespace Espada.Api.WebConsole.Mappings
{
    internal sealed record RememberMemoryMappingSource(
        Guid WorkspaceId,
        ConsoleRememberMemoryRequest Request,
        string ClientIdentity,
        string SessionIdentity);
}