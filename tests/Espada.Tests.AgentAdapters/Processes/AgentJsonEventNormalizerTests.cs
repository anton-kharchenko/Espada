using Espada.AgentAdapters.Processes;
using Espada.Domain.Enums;
using System.Text.Json;

namespace Espada.Tests.AgentAdapters.Processes
{
    public sealed class AgentJsonEventNormalizerTests
    {
        [Fact]
        public void NormalizeClaude_ShouldMapTextAndToolUse()
        {
            using JsonDocument document = JsonDocument.Parse(
                """{"type":"assistant","message":{"content":[{"type":"text","text":"answer"},{"type":"tool_use","name":"git"}]}}""");

            IReadOnlyList<Espada.AgentAdapters.Models.AgentProcessEvent> events =
                AgentJsonEventNormalizer.NormalizeClaude(document.RootElement);

            Assert.Collection(events,
                item => Assert.Equal(AgentSessionEventType.AssistantOutput, item.Type),
                item => Assert.Equal(AgentSessionEventType.ToolRequest, item.Type));
        }

        [Fact]
        public void NormalizeAcp_ShouldMapPermissionRelatedToolUpdates()
        {
            using JsonDocument document = JsonDocument.Parse(
                """{"sessionUpdate":"tool_call","toolCallId":"1","title":"git status"}""");

            Espada.AgentAdapters.Models.AgentProcessEvent processEvent =
                Assert.Single(AgentJsonEventNormalizer.NormalizeAcp(document.RootElement));

            Assert.Equal(AgentSessionEventType.ToolRequest, processEvent.Type);
        }
    }
}