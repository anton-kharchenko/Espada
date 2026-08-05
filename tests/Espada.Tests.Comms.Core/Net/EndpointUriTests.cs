using Espada.Comms.Core.Net;

namespace Espada.Tests.Comms.Core.Net
{
    public sealed class EndpointUriTests
    {
        [Theory]
        [MemberData(nameof(EndpointUriTestData.ValidEndpoints), MemberType = typeof(EndpointUriTestData))]
        public void TryCreate_WithHttpEndpoint_ReturnsUri(string value)
        {
            bool created = EndpointUri.TryCreate(value, out Uri? uri);

            Assert.True(created);
            Assert.NotNull(uri);
            Assert.Equal(value, uri.AbsoluteUri.TrimEnd('/'));
        }

        [Theory]
        [MemberData(nameof(EndpointUriTestData.InvalidEndpoints), MemberType = typeof(EndpointUriTestData))]
        public void TryCreate_WithInvalidEndpoint_ReturnsFalse(string? value)
        {
            bool created = EndpointUri.TryCreate(value, out Uri? uri);

            Assert.False(created);
            Assert.Null(uri);
        }

        [Fact]
        public void Create_WithInvalidEndpoint_IdentifiesSetting()
        {
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => EndpointUri.Create("relative", "Espada:Endpoint"));

            Assert.Contains("Espada:Endpoint", exception.Message, StringComparison.Ordinal);
        }
    }
}