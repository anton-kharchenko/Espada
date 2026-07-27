using Espada.Billing.Constants;

namespace Espada.Billing.Services;

internal static class BillingErrorSanitizer
{
    public static string Sanitize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string sanitized = value.Replace('\r', ' ').Replace('\n', ' ').Trim();

        return sanitized.Length <= BillingProcessingPolicy.MaximumSanitizedErrorLength ? sanitized : sanitized[..BillingProcessingPolicy.MaximumSanitizedErrorLength];
    }
}