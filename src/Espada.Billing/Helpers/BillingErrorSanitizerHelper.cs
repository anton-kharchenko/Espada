using Espada.Billing.Constants;

namespace Espada.Billing.Helpers;

internal static class BillingErrorSanitizerHelper
{
    public static string Sanitize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string sanitized = value.Replace('\r', ' ').Replace('\n', ' ').Trim();

        return sanitized.Length <= BillingProcessingConstnts.MaximumSanitizedErrorLength ? sanitized : sanitized[..BillingProcessingConstnts.MaximumSanitizedErrorLength];
    }
}