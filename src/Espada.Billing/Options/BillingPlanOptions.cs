namespace Espada.Billing.Options
{
    public sealed class BillingPlanOptions
    {
        public string PriceId { get; set; } = string.Empty;
        public long? IncludedStorageBytes { get; set; }
        public long? IncludedEmbeddingInputUnits { get; set; }
        public decimal? StorageByteHourRate { get; set; }
        public decimal? EmbeddingInputUnitRate { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PriceId)
                   && IncludedStorageBytes is >= 0
                   && IncludedEmbeddingInputUnits is >= 0
                   && StorageByteHourRate is >= 0
                   && EmbeddingInputUnitRate is >= 0;
        }
    }
}