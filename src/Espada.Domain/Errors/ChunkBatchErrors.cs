using Espada.Domain.Rules;

namespace Espada.Domain.Errors;

public static class ChunkBatchErrors
{
    public static readonly DomainError CannotStart = new("ChunkBatch.CannotStart", "Only a requested chunk batch can be started.");
    public static readonly DomainError CannotComplete = new("ChunkBatch.CannotComplete", "Only a running chunk batch can be completed.");
    public static readonly DomainError CannotFail = new("ChunkBatch.CannotFail", "Only a running chunk batch can be failed.");
    public static readonly DomainError ChunkCountInvalid = new("ChunkBatch.ChunkCount.Invalid", "Chunk count must be greater than zero.");
    public static readonly DomainError FailureReasonEmpty = new("ChunkBatch.FailureReason.Empty", "Chunk batch failure reason cannot be empty.");
}