using Espada.Protocol.Mcp.Contracts.Requests;

namespace Espada.Daemon.Models
{
    internal sealed record ContextSearchMappingSource(ContextSearchRequest Request, IReadOnlyList<float> QueryVector);
}