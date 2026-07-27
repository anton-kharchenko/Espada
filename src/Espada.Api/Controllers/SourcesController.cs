using Espada.Api.Contracts.Requests.Common;
using Espada.Api.Contracts.Requests.Sources;
using Espada.Application.UseCases.Sources.Commands.ArchiveSource;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Sources.Commands.SetSourcePriority;
using Espada.Application.UseCases.Sources.Common;
using Espada.Application.UseCases.Sources.Queries.GetSourceById;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Espada.Application.ApplicationErrors;
using Espada.Domain.ValueObjects.SourceDefinitions;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/sources")]
public sealed class SourcesController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(RegisterSourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromRoute] Guid workspaceId,
        [FromBody] RegisterSourceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(SourceApplicationErrors.InvalidName);
        }

        if (request.Definition is null or LegacySourceDefinition)
        {
            return BadRequest(SourceApplicationErrors.InvalidDefinition);
        }

        DomainResult<RegisterSourceResponse> result = await mediator.Send(
            new RegisterSourceCommand(
                workspaceId,
                request.Name,
                request.Definition!),
            cancellationToken);

        return result.IsFailure
            ? HandleError(result.Error)
            : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{sourceId:guid}")]
    [ProducesResponseType(typeof(SourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid sourceId,
        CancellationToken cancellationToken)
    {
        DomainResult<SourceResponse> result = await mediator.Send(
            new GetSourceByIdQuery(
                WorkspaceId: workspaceId,
                SourceId: sourceId),
            cancellationToken);

        return result.IsFailure
            ? HandleError(result.Error)
            : Ok(result.Value);
    }

    [HttpPost("{sourceId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid sourceId,
        CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(
            new ArchiveSourceCommand(
                WorkspaceId: workspaceId,
                SourceId: sourceId),
            cancellationToken);

        return result.IsFailure
            ? HandleError(result.Error)
            : NoContent();
    }

    [HttpPost("{sourceId:guid}/priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetPriority(
        [FromRoute] Guid workspaceId,
        [FromRoute] Guid sourceId,
        [FromBody] SetContextPriorityRequest request,
        CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(
            new SetSourcePriorityCommand(
                workspaceId,
                sourceId,
                request.Priority),
            cancellationToken);

        return result.IsFailure
            ? HandleError(result.Error)
            : NoContent();
    }
}