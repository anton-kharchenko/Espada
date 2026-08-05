using Espada.Api.Contracts.Requests.WebConsole;

namespace Espada.Api.Mappings.WebConsole
{
    internal sealed record CreateArtifactMappingSource(
        Guid WorkspaceId,
        ConsoleCreateArtifactRequest Request,
        bool AllowPolicyMutation);
}