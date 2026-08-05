using Espada.Domain.Constants;
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Espada.Infrastructure.Database
{
    internal static class SourceDefinitionSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        public static string Serialize(SourceDefinition definition)
        {
            string type = definition switch
            {
                FileSourceDefinition => SourceDefinitionDiscriminatorConstants.File,
                WebPageSourceDefinition => SourceDefinitionDiscriminatorConstants.WebPage,
                PlainTextSourceDefinition => SourceDefinitionDiscriminatorConstants.PlainText,
                ConversationSourceDefinition => SourceDefinitionDiscriminatorConstants.Conversation,
                ConnectorSourceDefinition => SourceDefinitionDiscriminatorConstants.Connector,
                RepositorySourceDefinition => SourceDefinitionDiscriminatorConstants.Repository,
                LegacySourceDefinition => SourceDefinitionDiscriminatorConstants.Legacy,
                _ => throw new JsonException("Source definition type is not supported.")
            };
            JsonObject payload =
                JsonSerializer.SerializeToNode(definition, definition.GetType(), SerializerOptions)?.AsObject() ??
                throw new JsonException("Source definition payload was empty.");
            payload.Insert(0, SourceDefinitionDiscriminatorConstants.Property, type);
            return payload.ToJsonString(SerializerOptions);
        }

        public static SourceDefinition Deserialize(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty(SourceDefinitionDiscriminatorConstants.Property,
                    out JsonElement type))
            {
                throw new JsonException("Source definition type is required.");
            }

            return type.GetString() switch
            {
                SourceDefinitionDiscriminatorConstants.File => Deserialize<FileSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.WebPage => Deserialize<WebPageSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.PlainText => Deserialize<PlainTextSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Conversation => Deserialize<ConversationSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Connector => Deserialize<ConnectorSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Repository => Deserialize<RepositorySourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Legacy => Deserialize<LegacySourceDefinition>(json),
                _ => throw new JsonException("Source definition type is not supported.")
            };
        }

        private static T Deserialize<T>(string json) where T : SourceDefinition
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions) ??
                   throw new JsonException("Source definition payload was empty.");
        }
    }
}