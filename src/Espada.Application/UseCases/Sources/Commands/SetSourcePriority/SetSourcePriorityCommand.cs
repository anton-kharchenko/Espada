using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Sources.Commands.SetSourcePriority
{
    public sealed record SetSourcePriorityCommand(Guid WorkspaceId, Guid SourceId, int Priority) : ICommand;
}