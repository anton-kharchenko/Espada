namespace Espada.Application.Models;

public sealed record BlobDescriptor(BlobHash Hash, long Length, string MediaType);