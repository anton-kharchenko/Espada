using Espada.Protocol.Mcp.Services;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace Espada.Protocol.Mcp.Resources
{
    internal static class McpResourceSerializer
    {
        private const string JsonMediaType = "application/json";

        private static readonly JsonSerializerOptions JsonOptions = new(
            JsonSerializerDefaults.Web) { WriteIndented = true };

        public static TextResourceContents Create<TData>(
            string uri,
            McpResourceProvenance provenance,
            TData data)
        {
            McpResourceDocument<TData> document = new(
                JsonMediaType,
                provenance,
                data);

            return new TextResourceContents
            {
                Uri = uri, MimeType = JsonMediaType, Text = JsonSerializer.Serialize(document, JsonOptions)
            };
        }

        public static Guid ParseId(string value, string parameterName)
        {
            if (!Guid.TryParse(value, out Guid id) || id == Guid.Empty)
            {
                throw McpErrorMapper.InvalidArgument(
                    $"{parameterName} must be a non-empty UUID.");
            }

            return id;
        }
    }
}