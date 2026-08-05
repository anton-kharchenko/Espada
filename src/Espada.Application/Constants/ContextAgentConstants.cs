namespace Espada.Application.Constants
{
    public static class ContextAgentConstants
    {
        public const string Codex = "codex";
        public const string Claude = "claude";
        public const string Gemini = "gemini";
        public const string Generic = "generic";

        public static bool IsSupported(string? agent)
        {
            return agent is not null
                   && (agent.Equals(Codex, StringComparison.OrdinalIgnoreCase)
                       || agent.Equals(Claude, StringComparison.OrdinalIgnoreCase)
                       || agent.Equals(Gemini, StringComparison.OrdinalIgnoreCase)
                       || agent.Equals(Generic, StringComparison.OrdinalIgnoreCase));
        }
    }
}