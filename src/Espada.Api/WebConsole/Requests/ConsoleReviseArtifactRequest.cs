using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Api.WebConsole.Requests
{
    internal sealed record ConsoleReviseArtifactRequest(
        string Content,
        IReadOnlyList<InstructionRuleInput>? InstructionRules = null,
        IReadOnlyList<PolicyRuleInput>? PolicyRules = null);
}