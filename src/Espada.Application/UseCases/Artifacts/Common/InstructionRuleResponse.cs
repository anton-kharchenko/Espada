namespace Espada.Application.UseCases.Artifacts.Common
{
    public sealed record InstructionRuleResponse(
        string RuleKey,
        string Text,
        int Priority);
}