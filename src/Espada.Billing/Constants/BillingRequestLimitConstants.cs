namespace Espada.Billing.Constants
{
    public static class BillingRequestLimitConstants
    {
        public const int MaximumIdempotencyKeyLength = 255;
        public const int MaximumWebhookPayloadBytes = 1_048_576;
        public const int MaximumWebhookSignatureLength = 8_192;
    }
}