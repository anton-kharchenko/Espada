using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class TaskStatusType(int id, string name) : Enumeration(id, name)
{
    public static readonly TaskStatusType Active = new(1, nameof(Active));

    public static readonly TaskStatusType Completed = new(2, nameof(Completed));

    public static readonly TaskStatusType Archived = new(3, nameof(Archived));
}