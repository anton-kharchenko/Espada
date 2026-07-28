using Espada.Infrastructure.Options;

namespace Espada.Tests.Infrastructure.TestData
{
    internal static class EmbeddingTestData
    {
        public const string BaseUrl = "http://localhost:11434";
        public const string ApiKey = "secret";
        public const string ModelIdentifier = "local-model";
        public const string ModelVersion = "1";
        public const string ProviderModel = "provider-model";
        public const string Input = "query";

        public static EmbeddingGenerationOptions CreateOptions(int dimensions)
        {
            return new EmbeddingGenerationOptions
            {
                BaseUrl = BaseUrl,
                ApiKey = ApiKey,
                Models =
                [
                    new EmbeddingModelOptions
                    {
                        Identifier = ModelIdentifier,
                        Version = ModelVersion,
                        ProviderModel = ProviderModel,
                        Dimensions = dimensions
                    }
                ]
            };
        }
    }
}