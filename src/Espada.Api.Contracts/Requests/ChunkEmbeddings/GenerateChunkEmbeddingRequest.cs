using Espada.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.ChunkEmbeddings;

public sealed class GenerateChunkEmbeddingRequest
{
    [Required, StringLength(EmbeddingModel.IdentifierMaxLength)]
    public string ModelIdentifier { get; init; } = string.Empty;

    [Required, StringLength(EmbeddingModel.VersionMaxLength)]
    public string ModelVersion { get; init; } = string.Empty;
}