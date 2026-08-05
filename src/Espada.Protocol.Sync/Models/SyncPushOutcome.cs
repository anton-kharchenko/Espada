using Espada.Protocol.Sync.Contracts;

namespace Espada.Protocol.Sync.Models
{
    public sealed record SyncPushOutcome(int StatusCode, SyncPushResponse? Response, string? Error);
}