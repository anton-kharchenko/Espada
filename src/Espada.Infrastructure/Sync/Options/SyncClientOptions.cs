namespace Espada.Infrastructure.Sync.Options
{
    public sealed class SyncClientOptions
    {
        public const string SectionName = "Sync:Client";

        public string ServerUrl { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public int PollIntervalSeconds { get; set; } = 30;
        public int MaxPushEvents { get; set; } = 200;
        public bool IncludeSessionTranscripts { get; set; }

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(ServerUrl)
                   && !string.IsNullOrWhiteSpace(Authority)
                   && !string.IsNullOrWhiteSpace(ClientId)
                   && !string.IsNullOrWhiteSpace(Scope);
        }

        public bool IsValid()
        {
            bool allEmpty = string.IsNullOrWhiteSpace(ServerUrl)
                            && string.IsNullOrWhiteSpace(Authority)
                            && string.IsNullOrWhiteSpace(ClientId)
                            && string.IsNullOrWhiteSpace(Scope);
            return allEmpty
                   || IsConfigured()
                   && Uri.TryCreate(ServerUrl, UriKind.Absolute, out Uri? server)
                   && server.Scheme == Uri.UriSchemeHttps
                   && Uri.TryCreate(Authority, UriKind.Absolute, out Uri? authority)
                   && authority.Scheme == Uri.UriSchemeHttps
                   && PollIntervalSeconds is >= 5 and <= 3600
                   && MaxPushEvents is >= 1 and <= 1000;
        }
    }
}