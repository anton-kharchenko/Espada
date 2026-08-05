namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleReviseArtifactRequest(
        string Content,
        IReadOnlyList<ConsoleInstructionRuleRequest>? InstructionRules = null,
        IReadOnlyList<ConsolePolicyRuleRequest>? PolicyRules = null);
}