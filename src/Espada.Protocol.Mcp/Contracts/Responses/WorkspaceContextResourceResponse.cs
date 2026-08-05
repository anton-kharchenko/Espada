using Espada.Application.UseCases.Context.Queries.BuildContext;

namespace Espada.Protocol.Mcp.Contracts.Responses
{
    public sealed record WorkspaceContextResourceResponse(
        Guid WorkspaceId,
        string ArtifactKind,
        IReadOnlyList<ContextItemResponse> Items);
}