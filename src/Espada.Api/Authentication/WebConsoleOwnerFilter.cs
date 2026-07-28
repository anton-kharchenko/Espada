using Espada.Api.Authentication.Constants;
using Espada.Api.WebConsole;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.ValueObjects;
using System.Security.Claims;

namespace Espada.Api.Authentication
{
    internal sealed class WebConsoleOwnerFilter(
        IWorkspaceMembershipRepository membershipRepository)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            Guid workspaceId = Guid.Parse(
                context.HttpContext.Request.RouteValues["workspaceId"]!
                    .ToString()!);
            ClaimsPrincipal user = context.HttpContext.User;
            string issuer = user.FindFirstValue(
                                WebConsoleAuthenticationConstants
                                    .IdentityIssuerClaim)
                            ?? string.Empty;
            string subject = user.FindFirstValue(
                                 WebConsoleAuthenticationConstants
                                     .IdentitySubjectClaim)
                             ?? string.Empty;
            bool isOwner = await membershipRepository.IsOwnerAsync(
                WorkspaceId.Create(workspaceId),
                issuer,
                subject,
                context.HttpContext.RequestAborted);
            if (!isOwner)
            {
                return WebConsoleResults.Forbidden(
                    "Workspace owner access is required.");
            }

            return await next(context);
        }
    }
}