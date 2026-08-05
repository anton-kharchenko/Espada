using Espada.Protocol.Sync;
using Espada.Protocol.Sync.Contracts;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Espada.Tests.Protocol.Sync.Contracts
{
    public sealed class SyncEnvelopeContractTests
    {
        [Fact]
        public void Serialize_ShouldPreserveVersionIdentitySequenceAndTypedPayload()
        {
            using JsonDocument payload = JsonDocument.Parse("""{"title":"Database rules","priority":5}""");
            string payloadHash = Convert.ToHexStringLower(
                SHA256.HashData(Encoding.UTF8.GetBytes(payload.RootElement.GetRawText())));
            SyncEnvelope expected = new(SyncProtocolConstants.Version, Guid.NewGuid(), Guid.NewGuid(), 42,
                Guid.NewGuid(), "Artifact", Guid.NewGuid(), "upsert", 3, DateTimeOffset.UtcNow, payloadHash,
                "Artifact.v1", payload.RootElement.Clone());

            string json = JsonSerializer.Serialize(expected, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            SyncEnvelope? actual = JsonSerializer.Deserialize<SyncEnvelope>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.NotNull(actual);
            Assert.Equal(1, actual.Version);
            Assert.Equal(expected.EventId, actual.EventId);
            Assert.Equal(expected.DeviceId, actual.DeviceId);
            Assert.Equal(42, actual.Sequence);
            Assert.Equal(expected.WorkspaceId, actual.WorkspaceId);
            Assert.Equal("Artifact.v1", actual.PayloadType);
            Assert.Equal(payloadHash, actual.PayloadHash);
            Assert.Equal("Database rules", actual.Payload.GetProperty("title").GetString());
        }
    }
}