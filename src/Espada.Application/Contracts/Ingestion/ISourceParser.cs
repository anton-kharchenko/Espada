namespace Espada.Application.Contracts.Ingestion
{
    public interface ISourceParser
    {
        Task<string> ParseAsync(
            Stream content,
            string fileName,
            string mediaType,
            CancellationToken cancellationToken = default);
    }
}