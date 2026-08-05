namespace Espada.Application.UseCases.Artifacts.Common
{
    public sealed record PolicyRuleInput(
        string RuleKey,
        string Text,
        int Priority,
        int EnforcementTypeId);
}