namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleCreateArtifactRequest(
        string Title,
        int TypeId,
        string Content,
        int KindTypeId,
        IReadOnlyList<ConsoleInstructionRuleRequest>? InstructionRules = null,
        IReadOnlyList<ConsolePolicyRuleRequest>? PolicyRules = null,
        bool IsDraft = false);
}