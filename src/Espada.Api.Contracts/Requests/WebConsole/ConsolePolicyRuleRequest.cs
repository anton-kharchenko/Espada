namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsolePolicyRuleRequest(
        string RuleKey,
        string Text,
        int Priority,
        int EnforcementTypeId);
}