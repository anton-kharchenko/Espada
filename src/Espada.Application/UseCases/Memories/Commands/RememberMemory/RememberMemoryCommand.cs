using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Memories.Commands.RememberMemory
{
    public sealed record RememberMemoryCommand(
        Guid WorkspaceId,
        string Title,
        string Content,
        int CategoryTypeId,
        decimal Confidence,
        string ClientIdentity,
        string? SessionIdentity = null,
        Guid? SupersededMemoryId = null) : ICommand<RememberMemoryResponse>;
}