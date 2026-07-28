namespace Espada.Api.Contracts.Responses.Billing
{
    public sealed record StripeWebhookReceiptResponse(bool Received, bool Duplicate);
}