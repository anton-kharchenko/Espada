using Espada.Api.Contracts.Requests.Artifacts;
using Espada.Api.Contracts.Requests.Common;
using Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Commands.RenameArtifact;
using Espada.Application.UseCases.Artifacts.Commands.SetArtifactPriority;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/artifacts")]
    public sealed class ArtifactsController(IMediator mediator) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateArtifactResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromRoute] Guid workspaceId, [FromBody] CreateArtifactRequest request,
            CancellationToken cancellationToken)
        {
            DomainResult<CreateArtifactResponse> result = await mediator.Send(
                new CreateArtifactCommand(workspaceId, request.Title, request.TypeId, request.Content),
                cancellationToken);

            return result.IsFailure
                ? HandleError(result.Error)
                : StatusCode(StatusCodes.Status201Created, result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ListArtifactsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> List([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
        {
            DomainResult<ListArtifactsResponse> result =
                await mediator.Send(new ListArtifactsQuery(workspaceId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }

        [HttpGet("{artifactId:guid}")]
        [ProducesResponseType(typeof(GetArtifactByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId,
            CancellationToken cancellationToken)
        {
            DomainResult<GetArtifactByIdResponse> result =
                await mediator.Send(new GetArtifactByIdQuery(workspaceId, artifactId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }

        [HttpPost("{artifactId:guid}/rename")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Rename([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId,
            [FromBody] RenameArtifactRequest request, CancellationToken cancellationToken)
        {
            DomainResult result = await mediator.Send(new RenameArtifactCommand(workspaceId, artifactId, request.Title),
                cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        [HttpPost("{artifactId:guid}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Archive([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId,
            CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(new ArchiveArtifactCommand(workspaceId, artifactId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        [HttpPost("{artifactId:guid}/priority")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetPriority([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId,
            [FromBody] SetContextPriorityRequest request, CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(new SetArtifactPriorityCommand(workspaceId, artifactId, request.Priority),
                    cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }
    }
}