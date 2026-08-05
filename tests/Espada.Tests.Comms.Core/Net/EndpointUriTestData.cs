namespace Espada.Tests.Comms.Core.Net
{
    public static class EndpointUriTestData
    {
        public static TheoryData<string> ValidEndpoints =>
        [
            "https://api.espada.dev",
            "http://localhost:7432/mcp"
        ];

        public static TheoryData<string?> InvalidEndpoints =>
        [
            null!,
            string.Empty,
            "/relative",
            "file:///tmp/espada"
        ];
    }
}