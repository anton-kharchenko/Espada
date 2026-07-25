using Espada.Api.Contracts.Requests.Imports;
using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Application.UseCases.Imports.Commands.CompleteImport;
using Espada.Application.UseCases.Imports.Commands.FailImport;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Imports.Commands.StartImport;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/imports")]
public sealed class ImportsController(IMediator mediator) : BaseController
{
    [HttpPost("sources/{sourceId:guid}")]
    [ProducesResponseType(typeof(RequestImportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestImport([FromRoute] Guid workspaceId, [FromRoute] Guid sourceId, CancellationToken cancellationToken)
    {
        DomainResult<RequestImportResponse> result = await mediator.Send(new RequestImportCommand(WorkspaceId: workspaceId, SourceId: sourceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("{importJobId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId, CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(new StartImportCommand(WorkspaceId: workspaceId, ImportJobId: importJobId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }

    [HttpPost("{importJobId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId, [FromBody] CompleteImportRequest request, CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(new CompleteImportCommand(WorkspaceId: workspaceId, ImportJobId: importJobId, ArtifactId: request.ArtifactId, ArtifactRevisionId: request.ArtifactRevisionId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }

    [HttpPost("{importJobId:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Fail([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId, [FromBody] FailImportRequest request, CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(new FailImportCommand(WorkspaceId: workspaceId, ImportJobId: importJobId, FailureCode: request.FailureCode, FailureReason: request.FailureReason), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }

    [HttpPost("{importJobId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId, CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(new CancelImportCommand(WorkspaceId: workspaceId, ImportJobId: importJobId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }

    [HttpGet("{importJobId:guid}")]
    [ProducesResponseType(typeof(GetImportByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId, CancellationToken cancellationToken)
    {
        DomainResult<GetImportByIdResponse> result = await mediator.Send(new GetImportByIdQuery(WorkspaceId: workspaceId, ImportJobId: importJobId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }
}