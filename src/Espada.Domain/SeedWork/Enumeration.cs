using System.Reflection;

namespace Espada.Domain.SeedWork;

public abstract class Enumeration(int id, string name) : IComparable
{
    public string Name { get; } = name;

    public int Id { get; } = id;

    public int CompareTo(object? other)
    {
        if (other is not Enumeration otherEnumeration)
        {
            throw new ArgumentException($"Object must be of type {nameof(Enumeration)}.", nameof(other));
        }

        return Id.CompareTo(otherEnumeration.Id);
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.GetValue(null))
            .Cast<T>();
    }

    public static T FromId<T>(int id) where T : Enumeration =>
        GetAll<T>().SingleOrDefault(value => value.Id == id)
        ?? throw new ArgumentOutOfRangeException(
            nameof(id),
            id,
            $"Unknown {typeof(T).Name} identifier.");

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Id);
    }
}