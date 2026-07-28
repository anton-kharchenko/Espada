using AutoMapper;
using Espada.Api.Contracts.Constants;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Espada.Comms.Core.Constants;

namespace Espada.Api.Controllers
{
    [Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/imports")]
    public sealed class ImportsController(IMediator mediator, IMapper mapper) : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(RequestImportResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RequestImport([FromRoute] Guid workspaceId,
            [FromHeader(Name = HttpHeaderNameConstants.IdempotencyKey)] [Required]
            string idempotencyKey,
            [FromBody] RequestImportRequest request, CancellationToken cancellationToken)
        {
            RequestImportCommand command =
                mapper.Map<RequestImportCommand>(new RequestImportMappingSource(workspaceId, idempotencyKey, request));
            DomainResult<RequestImportResponse> result = await mediator.Send(command, cancellationToken);

            return result.IsFailure
                ? HandleError(result.Error)
                : AcceptedAtAction(
                    nameof(GetById),
                    new { workspaceId, importJobId = result.Value.ImportJobId, version = ApiVersionConstants.V1 },
                    result.Value);
        }

        [HttpPost("{importJobId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cancel([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId,
            CancellationToken cancellationToken)
        {
            DomainResult result =
                await mediator.Send(new CancelImportCommand(workspaceId, importJobId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : NoContent();
        }

        [HttpGet("{importJobId:guid}")]
        [ProducesResponseType(typeof(GetImportByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid workspaceId, [FromRoute] Guid importJobId,
            CancellationToken cancellationToken)
        {
            DomainResult<GetImportByIdResponse> result =
                await mediator.Send(new GetImportByIdQuery(workspaceId, importJobId), cancellationToken);

            return result.IsFailure ? HandleError(result.Error) : Ok(result.Value);
        }
    }
}