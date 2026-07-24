using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors;

public static class ChunkErrors
{
    public static readonly DomainError ContentEmpty = new("Chunk.Content.Empty", "Chunk content cannot be empty.");

    public static readonly DomainError InvalidNumber = new("Chunk.Number.Invalid", "Chunk number must be greater than zero.");

    public static readonly DomainError SourceSpanStartInvalid = new("Chunk.SourceSpan.Start.Invalid", "Chunk source span start cannot be negative.");

    public static readonly DomainError SourceSpanLengthInvalid = new("Chunk.SourceSpan.Length.Invalid", "Chunk source span length must be greater than zero.");

    public static readonly DomainError SourceSpanOverflow = new("Chunk.SourceSpan.Overflow", "Chunk source span exceeds the supported text range.");

    public static readonly DomainError VersionEmpty = new("Chunk.Version.Empty", "Chunking version cannot be empty.");

    public static readonly DomainError VersionTooLong = new("Chunk.Version.TooLong", $"Chunking version cannot exceed {ChunkingVersion.MaxLength} characters.");
}