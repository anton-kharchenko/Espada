using Espada.Api.Contracts.Requests.ChunkEmbeddings;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.GenerateChunkEmbedding;
using Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/chunks/{chunkId:guid}/embedding")]
    public sealed class ChunkEmbeddingsController(IMediator mediator) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateChunkEmbeddingResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromRoute] Guid workspaceId, [FromRoute] Guid chunkId,
            [FromBody] CreateChunkEmbeddingRequest request, CancellationToken cancellationToken)
        {
            DomainResult<CreateChunkEmbeddingResponse> result = await mediator.Send(
                new CreateChunkEmbeddingCommand(workspaceId, chunkId, request.ModelIdentifier, request.ModelVersion,
                    request.Vector), cancellationToken);

            return result.IsFailure
                ? HandleError(result.Error)
                : StatusCode(StatusCodes.Status201Created, result.Value);
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(CreateChunkEmbeddingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Generate([FromRoute] Guid workspaceId, [FromRoute] Guid chunkId,
            [FromBody] GenerateChunkEmbeddingRequest request, CancellationToken cancellationToken)
        {
            DomainResult<CreateChunkEmbeddingResponse> result = await mediator.Send(
                new GenerateChunkEmbeddingCommand(workspaceId, chunkId, request.ModelIdentifier, request.ModelVersion),
                cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetChunkEmbeddingByChunkIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByChunkId([FromRoute] Guid workspaceId, [FromRoute] Guid chunkId,
            [FromQuery] string modelIdentifier, [FromQuery] string modelVersion, CancellationToken cancellationToken)
        {
            DomainResult<GetChunkEmbeddingByChunkIdResponse> result = await mediator.Send(
                new GetChunkEmbeddingByChunkIdQuery(workspaceId, chunkId, modelIdentifier, modelVersion),
                cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }
    }
}