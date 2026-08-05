using System.Text.Json;

namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record SourceRegisterRequest(
        Guid WorkspaceId,
        string Name,
        JsonElement Definition);
}