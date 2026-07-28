using Espada.Api.WebConsole.Requests;

namespace Espada.Api.WebConsole.Mappings
{
    internal sealed record CreateArtifactMappingSource(
        Guid WorkspaceId,
        ConsoleCreateArtifactRequest Request,
        bool AllowPolicyMutation);
}
