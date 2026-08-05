namespace Espada.Application.Models
{
    public sealed record RepositoryFileRecord(
        string RelativePath,
        string ContentHash,
        string FileName,
        string MediaType,
        long SizeInBytes);
}