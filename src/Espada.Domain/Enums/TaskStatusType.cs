using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class TaskStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly TaskStatusType Active = new(1, "active");

        public static readonly TaskStatusType Completed = new(2, "completed");

        public static readonly TaskStatusType Archived = new(3, "archived");
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