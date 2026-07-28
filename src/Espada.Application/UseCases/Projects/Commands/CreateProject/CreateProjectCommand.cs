using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.Projects.Commands.CreateProject
{
    public sealed record CreateProjectCommand(
        Guid WorkspaceId,
        string Name,
        string CanonicalRemoteUri,
        IReadOnlyList<string>? LocalAliases = null) : ICommand<ProjectResponse>;
}