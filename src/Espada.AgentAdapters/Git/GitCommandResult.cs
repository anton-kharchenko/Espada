namespace Espada.AgentAdapters.Git
{
    internal sealed record GitCommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public bool IsSuccess => ExitCode == 0;
    }
}