using Microsoft.Extensions.Configuration;

namespace Espada.Tests.Integration.TestData
{
    internal static class BillingTestConfigurationFactory
    {
        public static IConfiguration Create()
        {
            Dictionary<string, string?> values = new()
            {
                ["Billing:Enabled"] = "true",
                ["Billing:StripeSecretKey"] = "rk_test_placeholder",
                ["Billing:StripeWebhookSecret"] = "whsec_integration",
                ["Billing:CheckoutSuccessUrl"] = "https://example.test/success",
                ["Billing:CheckoutCancelUrl"] = "https://example.test/cancel",
                ["Billing:PortalReturnUrl"] = "https://example.test/billing",
                ["Billing:Solo:PriceId"] = "price_solo",
                ["Billing:Solo:IncludedStorageBytes"] = "1",
                ["Billing:Solo:IncludedEmbeddingInputUnits"] = "1",
                ["Billing:Solo:StorageByteHourRate"] = "0.1",
                ["Billing:Solo:EmbeddingInputUnitRate"] = "0.1",
                ["Billing:Team:PriceId"] = "price_team",
                ["Billing:Team:IncludedStorageBytes"] = "1",
                ["Billing:Team:IncludedEmbeddingInputUnits"] = "1",
                ["Billing:Team:StorageByteHourRate"] = "0.1",
                ["Billing:Team:EmbeddingInputUnitRate"] = "0.1",
                ["Billing:Usage:RawBytesEventName"] = "raw_bytes",
                ["Billing:Usage:ExtractedBytesEventName"] = "extracted_bytes",
                ["Billing:Usage:EmbeddingInputUnitsEventName"] = "embedding_input_units",
                ["Billing:Usage:ParserComputeMillisecondsEventName"] = "parser_compute_ms",
                ["Billing:Usage:PluginComputeMillisecondsEventName"] = "plugin_compute_ms",
                ["Billing:Usage:EgressBytesEventName"] = "egress_bytes",
                ["Billing:Usage:StorageByteHoursEventName"] = "storage_byte_hours"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}