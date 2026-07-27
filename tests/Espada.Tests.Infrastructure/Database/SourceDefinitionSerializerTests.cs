using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;

namespace Espada.Tests.Infrastructure.Database;

public sealed class SourceDefinitionSerializerTests
{
    [Fact]
    public void Serialize_ShouldPersistStableTypeDiscriminator()
    {
        string json = SourceDefinitionSerializer.Serialize(new WebPageSourceDefinition(new Uri("https://example.com/source")));
        Assert.Contains("\"type\":\"webPage\"", json, StringComparison.Ordinal);
        Assert.IsType<WebPageSourceDefinition>(SourceDefinitionSerializer.Deserialize(json));
    }
}