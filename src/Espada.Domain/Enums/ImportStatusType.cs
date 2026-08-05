using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class ImportStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly ImportStatusType Requested = new(1, nameof(Requested));

        public static readonly ImportStatusType Running = new(2, nameof(Running));

        public static readonly ImportStatusType Succeeded = new(3, nameof(Succeeded));

        public static readonly ImportStatusType Failed = new(4, nameof(Failed));

        public static readonly ImportStatusType Cancelled = new(5, nameof(Cancelled));
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