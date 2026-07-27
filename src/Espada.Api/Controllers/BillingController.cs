using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Billing;
using Espada.Api.Contracts.Responses.Billing;
using Espada.Billing.Constants;
using Espada.Billing.Models;
using Espada.Billing.UseCases.Checkout;
using Espada.Billing.UseCases.Portal;
using Espada.Billing.UseCases.Status;
using Espada.Billing.UseCases.Webhook;
using Espada.Comms.Core.Net;
using Espada.Domain.Rules;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}")]
public sealed class BillingController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpPost("workspaces/{workspaceId:guid}/billing/checkout")]
    [ProducesResponseType<HostedBillingSession>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreateCheckout([FromRoute] Guid workspaceId, [FromBody] CreateCheckoutRequest request, [FromHeader(Name = HttpHeaderNames.IdempotencyKey)] string idempotencyKey, CancellationToken cancellationToken)
    {
        CreateCheckoutCommand command = mapper.Map<CreateCheckoutCommand>(new CreateCheckoutMappingSource(workspaceId, request, idempotencyKey));
        DomainResult<HostedBillingSession> result = await mediator.Send(command, cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Accepted(result.Value);
    }

    [HttpPost("workspaces/{workspaceId:guid}/billing/portal")]
    [ProducesResponseType<HostedBillingSession>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreatePortal([FromRoute] Guid workspaceId, [FromHeader(Name = HttpHeaderNames.IdempotencyKey)] string idempotencyKey, CancellationToken cancellationToken)
    {
        DomainResult<HostedBillingSession> result = await mediator.Send(new CreateCustomerPortalCommand(workspaceId, idempotencyKey), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Accepted(result.Value);
    }

    [HttpGet("workspaces/{workspaceId:guid}/billing/status")]
    [ProducesResponseType<BillingStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromRoute] Guid workspaceId, CancellationToken cancellationToken)
    {
        DomainResult<BillingStatusSnapshot> result = await mediator.Send(new GetBillingStatusQuery(workspaceId), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(mapper.Map<BillingStatusResponse>(result.Value));
    }

    [AllowAnonymous]
    [HttpPost("billing/stripe/webhook")]
    [RequestSizeLimit(BillingRequestLimits.MaximumWebhookPayloadBytes)]
    [ProducesResponseType<StripeWebhookReceiptResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        using StreamReader reader = new(Request.Body);
        string payload = await reader.ReadToEndAsync(cancellationToken);
        string signature = Request.Headers[HttpHeaderNames.StripeSignature].ToString();
        DomainResult<StripeWebhookReceipt> result = await mediator.Send(new AcceptStripeWebhookCommand(payload, signature), cancellationToken);

        return result.IsFailure ? HandleError(result.Error) : Ok(mapper.Map<StripeWebhookReceiptResponse>(result.Value));
    }
}