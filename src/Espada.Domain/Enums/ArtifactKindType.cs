using Espada.Domain.SeedWork;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Espada.Domain.Enums
{
    [JsonConverter(typeof(ArtifactKindJsonConverter))]
    public sealed class ArtifactKindType(int id, string name) : Enumeration(id, name)
    {
        public static readonly ArtifactKindType Document = new(1, "document");

        public static readonly ArtifactKindType Instruction = new(2, "instruction");

        public static readonly ArtifactKindType Policy = new(3, "policy");

        public static readonly ArtifactKindType Memory = new(4, "memory");
    }

    public sealed class ArtifactKindJsonConverter : JsonConverter<ArtifactKindType>
    {
        public override ArtifactKindType Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            string? identifier = reader.GetString();
            return Enumeration.GetAll<ArtifactKindType>().SingleOrDefault(kind => kind.Name == identifier) ??
                   throw new JsonException($"Unknown artifact kindType '{identifier}'.");
        }

        public override void Write(Utf8JsonWriter writer, ArtifactKindType value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(value);
            writer.WriteStringValue(value.Name);
        }
    }
}