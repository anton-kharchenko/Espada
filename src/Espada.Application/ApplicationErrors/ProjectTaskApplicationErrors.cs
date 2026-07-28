using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ProjectTaskApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "Task.Id.Invalid",
            "Task ID cannot be empty.");

        public static DomainError NotFound(Guid taskId)
        {
            return new DomainError(
                "Task.NotFound",
                $"Task with ID '{taskId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid taskId, Guid workspaceId)
        {
            return new DomainError(
                "Task.NotFoundInWorkspace",
                $"Task with ID '{taskId:D}' was not found in workspace '{workspaceId:D}'.");
        }

        public static DomainError NotFoundInProject(Guid taskId, Guid projectId)
        {
            return new DomainError(
                "Task.NotFoundInProject",
                $"Task with ID '{taskId:D}' was not found in project '{projectId:D}'.");
        }
    }
}