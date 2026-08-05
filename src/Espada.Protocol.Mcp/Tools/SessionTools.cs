using Espada.Application.Contracts.Agents;
using Espada.Domain.ValueObjects;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Contracts.Responses;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Espada.Protocol.Mcp.Tools
{
    [McpServerToolType]
    public sealed class SessionTools(IAgentApprovalGateway approvals)
    {
        [McpServerTool(
            Name = "session.request_approval",
            Title = "Request tool approval",
            ReadOnly = false,
            Destructive = false,
            Idempotent = false,
            OpenWorld = false,
            UseStructuredContent = true,
            OutputSchemaType = typeof(AgentApprovalResponse))]
        [Description("Pauses a managed local agent session until the user approves or denies a tool call in Espada.")]
        public async Task<AgentApprovalResponse> RequestApprovalAsync(
            [Description("Tool name and exact structured arguments awaiting approval.")] AgentApprovalRequest request,
            CancellationToken cancellationToken)
        {
            string? sessionValue = Environment.GetEnvironmentVariable("ESPADA_AGENT_SESSION_ID");
            if (!Guid.TryParse(sessionValue, out Guid sessionId))
            {
                throw new InvalidOperationException(
                    "This approval tool is available only inside an Espada-managed agent session.");
            }

            bool approved = await approvals.RequestAsync(AgentSessionId.Create(sessionId), request.ToolName,
                request.Arguments.GetRawText(), cancellationToken);
            return new AgentApprovalResponse(approved);
        }
    }
}