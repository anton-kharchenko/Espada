using Espada.Application.Models;
using Espada.Protocol.Mcp.Contracts.Requests;

namespace Espada.Protocol.Mcp.Mappings
{
    internal sealed record MemoryRememberMappingSource(
        MemoryRememberRequest Request,
        RequestPrincipal Principal);
}