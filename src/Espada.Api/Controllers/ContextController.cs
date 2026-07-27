using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Context;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/context")]
public sealed class ContextController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchWorkspaceContextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Search([FromRoute] Guid workspaceId, [FromBody] SearchWorkspaceContextRequest request, CancellationToken cancellationToken)
    {
        DomainResult<SearchWorkspaceContextResponse> result = await mediator.Send(mapper.Map<SearchWorkspaceContextQuery>(new SearchWorkspaceContextMappingSource(workspaceId, request)), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
    }
}