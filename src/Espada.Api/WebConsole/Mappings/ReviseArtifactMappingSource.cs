using Espada.Api.WebConsole.Requests;

namespace Espada.Api.WebConsole.Mappings
{
    internal sealed record ReviseArtifactMappingSource(
        Guid WorkspaceId,
        Guid ArtifactId,
        ConsoleReviseArtifactRequest Request,
        bool AllowPolicyMutation,
        int? RequiredKindTypeId);
}
