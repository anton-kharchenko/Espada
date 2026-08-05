namespace Espada.Billing.Options
{
    public sealed class BillingOptions
    {
        public bool Enabled { get; set; }
        public string StripeSecretKey { get; set; } = string.Empty;
        public string StripeWebhookSecret { get; set; } = string.Empty;
        public Uri? CheckoutSuccessUrl { get; set; }
        public Uri? CheckoutCancelUrl { get; set; }
        public Uri? PortalReturnUrl { get; set; }
        public BillingPlanOptions Solo { get; set; } = new();
        public BillingPlanOptions Team { get; set; } = new();
        public BillingUsageOptions Usage { get; set; } = new();

        public bool IsValid()
        {
            return !Enabled
                   || (!string.IsNullOrWhiteSpace(StripeSecretKey)
                       && !string.IsNullOrWhiteSpace(StripeWebhookSecret)
                       && IsHttps(CheckoutSuccessUrl)
                       && IsHttps(CheckoutCancelUrl)
                       && IsHttps(PortalReturnUrl)
                       && Solo.IsValid()
                       && Team.IsValid()
                       && Usage.IsValid());
        }

        private static bool IsHttps(Uri? uri)
        {
            return uri is { IsAbsoluteUri: true } &&
                   uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }
    }
}