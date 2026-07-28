namespace Espada.Application.UseCases.Organizations.Common
{
    public sealed record OrganizationResponse(
        Guid Id,
        string Name,
        DateTimeOffset CreatedAtUtc);
}