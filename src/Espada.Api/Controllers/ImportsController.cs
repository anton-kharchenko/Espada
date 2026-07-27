using Espada.Api.Contracts.Requests.Imports;
using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Domain.Rules;
using Espada.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/workspaces/{workspaceId:guid}/imports")]
public sealed class ImportsController(IMediator mediator, IOptions<EmbeddingGenerationOptions> embeddingOptions) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(RequestImportResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestImport([FromRoute] Guid workspaceId, [FromHeader(Name = "Idempotency-Key"), Required] string idempotencyKey, [FromBody] RequestImportRequest request, CancellationToken cancellationToken)
    {
        ImportOptionsRequest requestedOptions = request.Options ?? new ImportOptionsRequest();
        ImportOptions options = new(requestedOptions.EmbeddingModel ?? embeddingOptions.Value.DefaultModel, requestedOptions.ChunkingStrategy, requestedOptions.MaxCharacters, requestedOptions.OverlapCharacters, requestedOptions.SemanticThreshold, requestedOptions.Separators, requestedOptions.CodeLanguage);
        DomainResult<RequestImportResponse> result = await mediator.Send(new RequestImportCommand(workspaceId, request.SourceId, idempotencyKey, options), cancellationToken);

        return result.IsFailure
            ? HandleError(result.Error)
            : AcceptedAtAction(nameof(GetById), new { workspaceId, importJobId = result.Value.ImportJobId, version = "1.0" }, result.Value);
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

        return result.IsFailure
            ? HandleError(result.Error)
            : Ok(result.Value);
    }
}