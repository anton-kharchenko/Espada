using Espada.Domain.Entities;

namespace Espada.Application.Models.Sync
{
    public sealed record StoredSyncEvent(SyncEvent Event, long ServerSequence);
}