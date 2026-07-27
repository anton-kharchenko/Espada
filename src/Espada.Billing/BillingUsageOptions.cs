using Espada.Application.Contracts.Billing.Constants;

namespace Espada.Billing;

public sealed class BillingUsageOptions
{
    public string RawBytesEventName { get; set; } = string.Empty;
    public string ExtractedBytesEventName { get; set; } = string.Empty;
    public string EmbeddingInputUnitsEventName { get; set; } = string.Empty;
    public string ParserComputeMillisecondsEventName { get; set; } = string.Empty;
    public string PluginComputeMillisecondsEventName { get; set; } = string.Empty;
    public string EgressBytesEventName { get; set; } = string.Empty;
    public string StorageByteHoursEventName { get; set; } = string.Empty;

    public bool IsValid() =>
        new[]
        {
            RawBytesEventName,
            ExtractedBytesEventName,
            EmbeddingInputUnitsEventName,
            ParserComputeMillisecondsEventName,
            PluginComputeMillisecondsEventName,
            EgressBytesEventName,
            StorageByteHoursEventName
        }.All(value => !string.IsNullOrWhiteSpace(value));

    public string GetEventName(string metric) => metric switch
    {
        UsageMetricConstants.RawBytes => RawBytesEventName,
        UsageMetricConstants.ExtractedBytes => ExtractedBytesEventName,
        UsageMetricConstants.EmbeddingInputUnits => EmbeddingInputUnitsEventName,
        UsageMetricConstants.ParserComputeMilliseconds => ParserComputeMillisecondsEventName,
        UsageMetricConstants.PluginComputeMilliseconds => PluginComputeMillisecondsEventName,
        UsageMetricConstants.EgressBytes => EgressBytesEventName,
        UsageMetricConstants.StorageByteHours => StorageByteHoursEventName,
        _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, null)
    };
}