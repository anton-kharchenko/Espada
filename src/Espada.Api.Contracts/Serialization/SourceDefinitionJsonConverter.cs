using Espada.Domain.Constants;
using Espada.Domain.ValueObjects.SourceDefinitions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Espada.Api.Contracts.Serialization
{
    public sealed class SourceDefinitionJsonConverter : JsonConverter<SourceDefinition>
    {
        public override SourceDefinition Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            if (!document.RootElement.TryGetProperty(SourceDefinitionDiscriminatorConstants.Property,
                    out JsonElement typeElement))
            {
                throw new JsonException("Source definition type is required.");
            }

            string json = document.RootElement.GetRawText();
            return typeElement.GetString() switch
            {
                SourceDefinitionDiscriminatorConstants.File => Deserialize<FileSourceDefinition>(json, options),
                SourceDefinitionDiscriminatorConstants.WebPage => Deserialize<WebPageSourceDefinition>(json, options),
                SourceDefinitionDiscriminatorConstants.PlainText => Deserialize<PlainTextSourceDefinition>(json,
                    options),
                SourceDefinitionDiscriminatorConstants.Conversation => Deserialize<ConversationSourceDefinition>(json,
                    options),
                SourceDefinitionDiscriminatorConstants.Connector => Deserialize<ConnectorSourceDefinition>(json,
                    options),
                _ => throw new JsonException("Source definition type is not supported.")
            };
        }

        public override void Write(Utf8JsonWriter writer, SourceDefinition value, JsonSerializerOptions options)
        {
            string type = value switch
            {
                FileSourceDefinition => SourceDefinitionDiscriminatorConstants.File,
                WebPageSourceDefinition => SourceDefinitionDiscriminatorConstants.WebPage,
                PlainTextSourceDefinition => SourceDefinitionDiscriminatorConstants.PlainText,
                ConversationSourceDefinition => SourceDefinitionDiscriminatorConstants.Conversation,
                ConnectorSourceDefinition => SourceDefinitionDiscriminatorConstants.Connector,
                _ => throw new JsonException("Source definition type is not supported.")
            };
            JsonElement element = JsonSerializer.SerializeToElement(value, value.GetType(), options);
            writer.WriteStartObject();
            writer.WriteString(SourceDefinitionDiscriminatorConstants.Property, type);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        private static T Deserialize<T>(string json, JsonSerializerOptions options) where T : SourceDefinition
        {
            return JsonSerializer.Deserialize<T>(json, options) ??
                   throw new JsonException("Source definition payload was empty.");
        }
    }
}