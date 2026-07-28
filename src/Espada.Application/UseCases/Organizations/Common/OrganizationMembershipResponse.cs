namespace Espada.Application.UseCases.Organizations.Common
{
    public sealed record OrganizationMembershipResponse(
        Guid Id,
        Guid OrganizationId,
        string Issuer,
        string Subject,
        int RoleTypeId,
        string RoleTypeName,
        DateTimeOffset JoinedAtUtc);
}