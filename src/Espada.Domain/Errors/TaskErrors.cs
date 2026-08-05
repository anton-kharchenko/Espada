using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class TaskErrors
    {
        public static DomainError TitleEmpty { get; } = new("Task.TitleEmpty", "Task title cannot be empty.");

        public static DomainError TitleTooLong { get; } =
            new("Task.TitleTooLong", "Task title cannot exceed 500 characters.");

        public static DomainError NotActive { get; } = new("Task.NotActive", "Only an active task can be completed.");
        public static DomainError AlreadyArchived { get; } = new("Task.AlreadyArchived", "Task is already archived.");
    }
}