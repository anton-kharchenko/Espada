using Espada.Application.Constants;

namespace Espada.Tests.AgentAdapters.TestData
{
    public static class AgentContextProjectionTestData
    {
        public static TheoryData<string, string> GoldenFiles =>
        [
            (ContextAgentConstants.Codex, "codex.txt"),
            (ContextAgentConstants.Claude, "claude.txt"),
            (ContextAgentConstants.Gemini, "gemini.txt"),
            (ContextAgentConstants.Generic, "generic.json")
        ];
    }
}