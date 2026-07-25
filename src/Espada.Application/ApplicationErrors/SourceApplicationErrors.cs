using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class SourceApplicationErrors
    {
        public static readonly DomainError InvalidId = new("Source.Id.Invalid", "Source ID cannot be empty.");

        public static DomainError NotFound(Guid sourceId) => new("Source.NotFound", $"Source with ID '{sourceId:D}' was not found.");

        public static DomainError NotFoundInWorkspace(Guid sourceId, Guid workspaceId) => new("Source.NotFoundInWorkspace", $"Source with ID '{sourceId:D}' was not found in workspace '{workspaceId:D}'.");
    }
}