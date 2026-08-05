namespace Espada.AgentAdapters.Models
{
    public sealed record AgentProcessRequest(Guid SessionId, string ExecutablePath, string WorkingDirectory,
        string Prompt);
}