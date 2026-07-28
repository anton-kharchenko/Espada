namespace Espada.Billing.Constants;

public static class BillingProcessingConstnts
{
    public const int MaximumRetryAttempts = 5;
    public const int MaximumSanitizedErrorLength = 1000;
    public const string DefaultGuidFormat = "D";
    public const string CompactGuidFormat = "N";

    public static TimeSpan LeaseDuration { get; } = TimeSpan.FromMinutes(2);

    public static IReadOnlyList<TimeSpan> WebhookRetryDelays { get; } =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    ];

    public static TimeSpan GetUsageRetryDelay(int attempt) => TimeSpan.FromMinutes(Math.Pow(2, attempt - 1));
}