using Espada.Api.Contracts.Requests.WebConsole;

namespace Espada.Api.Mappings.WebConsole
{
    internal sealed record ReviseArtifactMappingSource(
        Guid WorkspaceId,
        Guid ArtifactId,
        ConsoleReviseArtifactRequest Request,
        bool AllowPolicyMutation,
        int? RequiredKindTypeId);
}