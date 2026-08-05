using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class MemoryCategoryType(int id, string name) : Enumeration(id, name)
    {
        public static readonly MemoryCategoryType Fact = new(1, nameof(Fact));

        public static readonly MemoryCategoryType Decision = new(2, nameof(Decision));

        public static readonly MemoryCategoryType Preference = new(3, nameof(Preference));

        public static readonly MemoryCategoryType Episode = new(4, nameof(Episode));

        public static readonly MemoryCategoryType Summary = new(5, nameof(Summary));

        public static readonly MemoryCategoryType Observation = new(6, nameof(Observation));

        public static readonly MemoryCategoryType Warning = new(7, nameof(Warning));
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