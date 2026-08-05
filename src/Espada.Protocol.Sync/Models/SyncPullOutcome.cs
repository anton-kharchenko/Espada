using Espada.Protocol.Sync.Contracts;

namespace Espada.Protocol.Sync.Models
{
    public sealed record SyncPullOutcome(int StatusCode, SyncPullResponse? Response, string? Error);
}