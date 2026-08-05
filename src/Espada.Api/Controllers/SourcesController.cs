using AutoMapper;
using Espada.Api.Contracts.Models;
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

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/sources")]
    public sealed class SourcesController(IMediator mediator, IMapper mapper) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(RegisterSourceResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Register([FromRoute] Guid workspaceId,
            [FromBody] RegisterSourceRequest request, CancellationToken cancellationToken)
        {
            RegisterSourceCommand command =
                mapper.Map<RegisterSourceCommand>(new RegisterSourceMappingSource(workspaceId, request));
            DomainResult<RegisterSourceResponse> result = await mediator.Send(command, cancellationToken);

            return result.IsFailure
                ? HandleError(result.Error)
                : StatusCode(StatusCodes.Status201Created, result.Value);
        }

        [HttpGet("{sourceId:guid}")]
        [ProducesResponseType(typeof(SourceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid sourceId,
            CancellationToken cancellationToken)
        {
            DomainResult<SourceResponse> result =
                await mediator.Send(new GetSourceByIdQuery(workspaceId, sourceId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }

        [HttpPost("{sourceId:guid}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Archive([FromRoute] Guid workspaceId, [FromRoute] Guid sourceId,
            CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(new ArchiveSourceCommand(workspaceId, sourceId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        [HttpPost("{sourceId:guid}/priority")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetPriority([FromRoute] Guid workspaceId, [FromRoute] Guid sourceId,
            [FromBody] SetContextPriorityRequest request, CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(new SetSourcePriorityCommand(workspaceId, sourceId, request.Priority),
                    cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }
    }
}