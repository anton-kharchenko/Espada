using System.Text.Json;

namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record AgentApprovalRequest(string ToolName, JsonElement Arguments);
}