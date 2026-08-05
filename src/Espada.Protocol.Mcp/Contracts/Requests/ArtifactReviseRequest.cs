using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record ArtifactReviseRequest(
        Guid WorkspaceId,
        Guid ArtifactId,
        string Content,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null);
}