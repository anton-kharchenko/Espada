namespace Espada.Application.Models;

public sealed record SourceReadResult(
    Stream Content,
    string FileName,
    string MediaType);