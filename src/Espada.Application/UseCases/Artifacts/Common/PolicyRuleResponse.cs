namespace Espada.Application.UseCases.Artifacts.Common
{
    public sealed record PolicyRuleResponse(
        string RuleKey,
        string Text,
        int Priority,
        int EnforcementTypeId,
        string EnforcementTypeName);
}