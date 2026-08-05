namespace Espada.Infrastructure.Options
{
    public sealed class BlobStorageOptions
    {
        public string Provider { get; set; } = "FileSystem";

        public string? AzureContainerUri { get; set; }
    }
}