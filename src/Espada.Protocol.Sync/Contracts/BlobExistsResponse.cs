namespace Espada.Protocol.Sync.Contracts
{
    public sealed record BlobExistsResponse(string Hash, bool Exists);
}