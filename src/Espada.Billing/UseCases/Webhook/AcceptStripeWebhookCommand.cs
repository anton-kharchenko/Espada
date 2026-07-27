using Espada.Application.Contracts.Messaging;
using Espada.Billing.Models;

namespace Espada.Billing.UseCases.Webhook;

public sealed record AcceptStripeWebhookCommand(
    string Payload,
    string Signature) : ICommand<StripeWebhookReceipt>;