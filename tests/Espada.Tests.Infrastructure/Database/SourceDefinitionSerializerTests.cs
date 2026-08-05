using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Infrastructure.Database;

namespace Espada.Tests.Infrastructure.Database
{
    public sealed class SourceDefinitionSerializerTests
    {
        [Fact]
        public void Serialize_ShouldPersistStableTypeDiscriminator()
        {
            string json =
                SourceDefinitionSerializer.Serialize(
                    new WebPageSourceDefinition(new Uri("https://example.com/source")));
            Assert.Contains("\"type\":\"webPage\"", json, StringComparison.Ordinal);
            Assert.IsType<WebPageSourceDefinition>(SourceDefinitionSerializer.Deserialize(json));
        }

        [Fact]
        public void Serialize_Repository_ShouldRoundTripScanPolicy()
        {
            RepositorySourceDefinition definition = new(
                "11111111-1111-1111-1111-111111111111",
                null,
                new RepositoryScanPolicy(2048));

            string json = SourceDefinitionSerializer.Serialize(definition);
            RepositorySourceDefinition restored = Assert.IsType<RepositorySourceDefinition>(
                SourceDefinitionSerializer.Deserialize(json));

            Assert.Contains("\"type\":\"repository\"", json, StringComparison.Ordinal);
            Assert.Equal(definition, restored);
        }
    }
}