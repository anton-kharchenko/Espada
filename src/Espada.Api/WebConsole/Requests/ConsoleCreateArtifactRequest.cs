using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleCreateArtifactRequest(
        string Title,
        int TypeId,
        string Content,
        int KindTypeId,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null);
}
