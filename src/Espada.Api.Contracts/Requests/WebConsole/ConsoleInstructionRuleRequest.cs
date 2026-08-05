namespace Espada.Api.Contracts.Requests.WebConsole
{
    public sealed record ConsoleInstructionRuleRequest(
        string RuleKey,
        string Text,
        int Priority);
}