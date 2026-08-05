using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision
{
    public sealed record ListChunksByRevisionQuery(Guid WorkspaceId, Guid ArtifactRevisionId)
        : IQuery<ListChunksByRevisionResponse>;
}