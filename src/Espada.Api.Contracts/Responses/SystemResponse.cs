namespace Espada.Api.Contracts.Responses;

public sealed record SystemResponse(string Service, string Status, DateTimeOffset UtcNow);