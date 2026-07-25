using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Sources.Commands.ArchiveSource
{
    public sealed record ArchiveSourceCommand(Guid WorkspaceId, Guid SourceId) : ICommand;
}