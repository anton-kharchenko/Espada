using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    public sealed record RequestImportCommand(Guid WorkspaceId, Guid SourceId) : ICommand<RequestImportResponse>;
}