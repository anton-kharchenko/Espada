namespace Espada.Application.UseCases.Artifacts.Common
{
    public sealed record InstructionRuleInput(
        string RuleKey,
        string Text,
        int Priority);
}