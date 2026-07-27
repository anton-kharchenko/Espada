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
        "raw_bytes" => RawBytesEventName,
        "extracted_bytes" => ExtractedBytesEventName,
        "embedding_input_units" => EmbeddingInputUnitsEventName,
        "parser_compute_milliseconds" => ParserComputeMillisecondsEventName,
        "plugin_compute_milliseconds" => PluginComputeMillisecondsEventName,
        "egress_bytes" => EgressBytesEventName,
        "storage_byte_hours" => StorageByteHoursEventName,
        _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, null)
    };
}