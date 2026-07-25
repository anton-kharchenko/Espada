using Espada.Api.Contracts.Requests.Chunks;
using Espada.Application.UseCases.Chunks.Commands.CreateChunks;
using Espada.Application.UseCases.Chunks.Queries.GetChunkById;
using Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}")]
public sealed class ChunksController(IMediator mediator) : BaseController
{
    [HttpPost("chunk-batches/{chunkBatchId:guid}/chunks")]
    [ProducesResponseType(typeof(CreateChunksResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromRoute] Guid workspaceId, [FromRoute] Guid chunkBatchId, [FromBody] CreateChunksRequest request, CancellationToken cancellationToken)
    {
        CreateChunkItem[] items = request.Items.Select(item => new CreateChunkItem(Number: item.Number, Content: item.Content, SourceStart: item.SourceStart, SourceLength: item.SourceLength)).ToArray();

        DomainResult<CreateChunksResponse> result = await mediator.Send(new CreateChunksCommand(WorkspaceId: workspaceId, ChunkBatchId: chunkBatchId, Items: items), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("chunks/{chunkId:guid}")]
    [ProducesResponseType(typeof(GetChunkByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid chunkId, CancellationToken cancellationToken)
    {
        DomainResult<GetChunkByIdResponse> result = await mediator.Send(new GetChunkByIdQuery(WorkspaceId: workspaceId, ChunkId: chunkId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }

    [HttpGet("artifact-revisions/{artifactRevisionId:guid}/chunks")]
    [ProducesResponseType(typeof(ListChunksByRevisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByRevision([FromRoute] Guid workspaceId, [FromRoute] Guid artifactRevisionId, CancellationToken cancellationToken)
    {
        DomainResult<ListChunksByRevisionResponse> result = await mediator.Send(new ListChunksByRevisionQuery(WorkspaceId: workspaceId, ArtifactRevisionId: artifactRevisionId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }
}