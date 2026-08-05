using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class SourceStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly SourceStatusType Active = new(1, nameof(Active));

        public static readonly SourceStatusType Archived = new(2, nameof(Archived));
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