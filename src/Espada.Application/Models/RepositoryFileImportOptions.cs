namespace Espada.Application.Models
{
    public sealed record RepositoryFileImportOptions(
        string RepositoryRoot,
        string RelativePath,
        string ContentHash,
        string FileName,
        string MediaType,
        long SizeInBytes);
}