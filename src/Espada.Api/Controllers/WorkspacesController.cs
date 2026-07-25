using Espada.Domain.Enums;
using Espada.Api.Extensions;
using Espada.Api.Contracts.Requests.Workspaces;
using Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces")]
public sealed class WorkspacesController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        WorkspaceType workspaceType = request.TypeId.ToEnumeration<WorkspaceType>()
            ?? throw new InvalidOperationException($"Workspace type ID '{request.TypeId}' passed validation but could not be resolved.");

        DomainResult<CreateWorkspaceResponse> result = await mediator.Send(new CreateWorkspaceCommand(request.Name, workspaceType), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{workspaceId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid workspaceId, CancellationToken cancellationToken)
    {
        DomainResult<WorkspaceResponse> result = await mediator.Send(new GetWorkspaceByIdQuery(workspaceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }

    [HttpPost("{workspaceId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid workspaceId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveWorkspaceCommand(workspaceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }
}