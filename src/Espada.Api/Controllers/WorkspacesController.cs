using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Workspaces;
using Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces")]
public sealed class WorkspacesController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateWorkspaceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        bool externalIdentity = User.Identity?.AuthenticationType == JwtBearerDefaults.AuthenticationScheme;

        CreateWorkspaceCommand command = mapper.Map<CreateWorkspaceCommand>(
            new CreateWorkspaceMappingSource(
                request,
                externalIdentity ? User.FindFirst("iss")?.Value : null,
                externalIdentity ? User.FindFirst("sub")?.Value : null));

        DomainResult<CreateWorkspaceResponse> result = await mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{workspaceId:guid}")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
    {
        DomainResult<WorkspaceResponse> result = await mediator.Send(new GetWorkspaceByIdQuery(WorkspaceId: workspaceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }

    [HttpPost("{workspaceId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
    {
        DomainResult result = await mediator.Send(new ArchiveWorkspaceCommand(WorkspaceId: workspaceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : NoContent();
    }
}