using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class SyncConflictStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly SyncConflictStatusType Open = new(1, nameof(Open));

        public static readonly SyncConflictStatusType Resolved = new(2, nameof(Resolved));

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
