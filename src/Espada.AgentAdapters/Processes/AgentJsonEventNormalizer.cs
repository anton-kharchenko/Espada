using Espada.AgentAdapters.Models;
using Espada.Domain.Enums;
using System.Text.Json;

namespace Espada.AgentAdapters.Processes
{
    internal static class AgentJsonEventNormalizer
    {
        public static IReadOnlyList<AgentProcessEvent> NormalizeClaude(JsonElement message)
        {
            List<AgentProcessEvent> events = [];
            string? type = GetString(message, "type");
            if (type == "assistant" && message.TryGetProperty("message", out JsonElement assistant))
            {
                AddClaudeContent(events, assistant);
            }
            else if (type == "user" && message.TryGetProperty("message", out JsonElement user))
            {
                AddClaudeContent(events, user);
            }
            else if (type == "result")
            {
                events.Add(new AgentProcessEvent(AgentSessionEventType.Usage, message.GetRawText()));
            }
            else if (type == "system")
            {
                events.Add(new AgentProcessEvent(AgentSessionEventType.Status, message.GetRawText()));
            }

            return events;
        }

        public static IReadOnlyList<AgentProcessEvent> NormalizeCodex(string method, JsonElement parameters)
        {
            AgentSessionEventType type = method switch
            {
                var value when value.Contains("completed", StringComparison.OrdinalIgnoreCase) =>
                    AgentSessionEventType.Status,
                var value when value.Contains("diff", StringComparison.OrdinalIgnoreCase) =>
                    AgentSessionEventType.DiffUpdate,
                var value when value.Contains("command", StringComparison.OrdinalIgnoreCase) =>
                    AgentSessionEventType.ToolResult,
                var value when value.Contains("usage", StringComparison.OrdinalIgnoreCase) =>
                    AgentSessionEventType.Usage,
                var value when value.Contains("error", StringComparison.OrdinalIgnoreCase) =>
                    AgentSessionEventType.Error,
                _ => AgentSessionEventType.AssistantOutput
            };
            return [new AgentProcessEvent(type, parameters.GetRawText())];
        }

        public static IReadOnlyList<AgentProcessEvent> NormalizeAcp(JsonElement update)
        {
            string? kind = GetString(update, "sessionUpdate") ?? GetString(update, "type");
            AgentSessionEventType type = kind switch
            {
                "agent_message_chunk" => AgentSessionEventType.AssistantOutput,
                "tool_call" => AgentSessionEventType.ToolRequest,
                "tool_call_update" => AgentSessionEventType.ToolResult,
                "plan" => AgentSessionEventType.Status,
                "usage_update" => AgentSessionEventType.Usage,
                _ => AgentSessionEventType.Status
            };
            return [new AgentProcessEvent(type, update.GetRawText())];
        }

        private static void AddClaudeContent(List<AgentProcessEvent> events, JsonElement message)
        {
            if (!message.TryGetProperty("content", out JsonElement content) || content.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (JsonElement item in content.EnumerateArray())
            {
                string? itemType = GetString(item, "type");
                AgentSessionEventType? eventType = itemType switch
                {
                    "text" => AgentSessionEventType.AssistantOutput,
                    "tool_use" => AgentSessionEventType.ToolRequest,
                    "tool_result" => AgentSessionEventType.ToolResult,
                    _ => null
                };
                if (eventType is not null)
                {
                    events.Add(new AgentProcessEvent(eventType, item.GetRawText()));
                }
            }
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object
                   && element.TryGetProperty(propertyName, out JsonElement property)
                   && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }
    }
}