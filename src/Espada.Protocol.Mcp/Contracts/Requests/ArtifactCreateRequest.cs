using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record ArtifactCreateRequest(
        Guid WorkspaceId,
        string Title,
        int TypeId,
        string Content,
        int KindTypeId = 1,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null);
}