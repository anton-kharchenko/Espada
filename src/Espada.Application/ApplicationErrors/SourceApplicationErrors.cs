using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class SourceApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "Source.Id.Invalid",
            "Source ID cannot be empty.");

        public static readonly DomainError InvalidDefinition = new(
            "Source.Definition.Invalid",
            "Source definition is required.");

        public static readonly DomainError InvalidName = new(
            "Source.Name.Invalid",
            "Source name is required.");

        public static DomainError NotFound(Guid sourceId)
        {
            return new DomainError(
                "Source.NotFound",
                $"Source with ID '{sourceId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(
            Guid sourceId,
            Guid workspaceId)
        {
            return new DomainError(
                "Source.NotFoundInWorkspace",
                $"Source with ID '{sourceId:D}' was not found in workspace '{workspaceId:D}'.");
        }
    }
}