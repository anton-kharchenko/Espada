using Espada.Application.Models;
using Espada.Protocol.Mcp.Contracts.Requests;

namespace Espada.Protocol.Mcp.Mappings
{
    internal sealed record WorkspaceCreateMappingSource(
        WorkspaceCreateRequest Request,
        RequestPrincipal Principal);
}