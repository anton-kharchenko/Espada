using Espada.Api.Contracts.Requests.ArtifactRevisions;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/artifacts/{artifactId:guid}/revisions")]
public sealed class ArtifactRevisionsController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(AddArtifactRevisionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId, [FromBody] AddArtifactRevisionRequest request, CancellationToken cancellationToken)
    {
        DomainResult<AddArtifactRevisionResponse> result = await mediator.Send(new AddArtifactRevisionCommand(WorkspaceId: workspaceId, ArtifactId: artifactId, Content: request.Content), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListArtifactRevisionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId, CancellationToken cancellationToken)
    {
        DomainResult<ListArtifactRevisionsResponse> result = await mediator.Send(new ListArtifactRevisionsQuery(WorkspaceId: workspaceId, ArtifactId: artifactId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }

    [HttpGet("{artifactRevisionId:guid}")]
    [ProducesResponseType(typeof(GetArtifactRevisionByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid artifactId, [FromRoute] Guid artifactRevisionId, CancellationToken cancellationToken)
    {
        DomainResult<GetArtifactRevisionByIdResponse> result = await mediator.Send(new GetArtifactRevisionByIdQuery(WorkspaceId: workspaceId, ArtifactId: artifactId, ArtifactRevisionId: artifactRevisionId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }
}
