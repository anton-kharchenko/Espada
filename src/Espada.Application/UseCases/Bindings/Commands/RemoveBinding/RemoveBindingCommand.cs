using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Bindings.Commands.RemoveBinding
{
    public sealed record RemoveBindingCommand(
        Guid WorkspaceId,
        Guid BindingId) : ICommand;
}