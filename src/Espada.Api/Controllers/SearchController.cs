using Espada.Application.UseCases.Search.Queries.UnifiedSearch;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/search")]
    public sealed class SearchController(IMediator mediator) : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(UnifiedSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Search([FromRoute] Guid workspaceId, [FromQuery] string query,
            [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
        {
            DomainResult<UnifiedSearchResponse> result = await mediator.Send(
                new UnifiedSearchQuery(workspaceId, query, limit), cancellationToken);
            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }
    }
}