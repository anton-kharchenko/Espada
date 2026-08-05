namespace Espada.AgentAdapters.Execution
{
    internal sealed record PendingAgentApproval(TaskCompletionSource<bool> Completion);
}