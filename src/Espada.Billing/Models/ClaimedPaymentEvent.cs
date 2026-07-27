namespace Espada.Billing.Models;

public sealed record ClaimedPaymentEvent(string ProviderEventId, string EventType, string PayloadJson, int Attempt);