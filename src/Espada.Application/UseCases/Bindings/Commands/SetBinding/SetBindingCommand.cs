using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Bindings.Common;

namespace Espada.Application.UseCases.Bindings.Commands.SetBinding
{
    public sealed record SetBindingCommand(
        Guid WorkspaceId,
        Guid ArtifactId,
        Guid? BindingId = null,
        Guid? OrganizationId = null,
        Guid? ProjectId = null,
        string? RepositoryCanonicalUri = null,
        string? RepositoryRelativePathPrefix = null,
        string? Branch = null,
        Guid? TaskId = null,
        string? Agent = null) : ICommand<BindingResponse>;
}