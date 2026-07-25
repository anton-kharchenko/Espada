using Espada.Api.Contracts.Requests.ChunkBatches;
using Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/artifacts/{artifactId:guid}/revisions/{artifactRevisionId:guid}/chunk-batches")]
public sealed class ChunkBatchesController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateChunkBatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId, [FromRoute] Guid artifactRevisionId, [FromBody] CreateChunkBatchRequest request, CancellationToken cancellationToken)
    {
        DomainResult<CreateChunkBatchResponse> result = await mediator.Send(new CreateChunkBatchCommand(WorkspaceId: workspaceId, ArtifactId: artifactId, ArtifactRevisionId: artifactRevisionId, StrategyId: request.StrategyId, StrategyVersion: request.StrategyVersion), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }
}