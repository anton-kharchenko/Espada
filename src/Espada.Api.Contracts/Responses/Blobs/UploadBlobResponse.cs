namespace Espada.Api.Contracts.Responses.Blobs
{
    public sealed record UploadBlobResponse(string BlobHash, string FileName, string MediaType, long Length);
}