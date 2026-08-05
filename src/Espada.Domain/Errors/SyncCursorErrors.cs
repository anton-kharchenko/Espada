using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class SyncCursorErrors
    {
        public static readonly DomainError PushSequenceCannotMoveBackward = new("SyncCursor.PushSequenceCannotMoveBackward",
            "The acknowledged push sequence cannot move backward.");
        public static DomainError ServerCursorEmpty { get; } = new("SyncCursor.ServerCursorEmpty",
            "Sync server cursor cannot be empty.");
    }
}