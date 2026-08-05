namespace Aspire.Hosting.Espada.Constants
{
    internal static class EspadaConfigurationKeyConstants
    {
        public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
        public const string DotNetEnvironment = "DOTNET_ENVIRONMENT";
        public const string ParametersSectionPrefix = "Parameters:";
        public const string ApiKey = "Authentication__ApiKey__Value";
        public const string BlobRoot = "Ingestion__BlobRoot";
        public const string EmbeddingBaseUrl = "EmbeddingGeneration__BaseUrl";
        public const string EmbeddingDefaultModel = "EmbeddingGeneration__DefaultModel";
        public const string BillingStripeSecretKey = "Billing__StripeSecretKey";
        public const string BillingStripeWebhookSecret = "Billing__StripeWebhookSecret";
        public const string AppHostDisableStripe = "AppHost:DisableStripe";
        public const string LocalRuntimeEnabled = "Espada__LocalRuntime__Enabled";
    }
}