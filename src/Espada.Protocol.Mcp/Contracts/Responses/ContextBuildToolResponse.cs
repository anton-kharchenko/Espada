using Espada.AgentAdapters.Context;
using Espada.Application.UseCases.Context.Queries.BuildContext;

namespace Espada.Protocol.Mcp.Contracts.Responses
{
    public sealed record ContextBuildToolResponse(
        BuildContextResponse Context,
        AgentContextProjection Projection);
}