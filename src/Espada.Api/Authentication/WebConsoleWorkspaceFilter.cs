using Espada.Api.Authentication.Constants;
using Espada.Api.Extensions;
using Espada.Application.Policies;
using Espada.Domain.Rules;
using System.Security.Claims;

namespace Espada.Api.Authentication
{
    internal sealed class WebConsoleWorkspaceFilter(
        WorkspaceAccessPolicy accessPolicy) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            if (!Guid.TryParse(
                    context.HttpContext.Request.RouteValues["workspaceId"]
                        ?.ToString(),
                    out Guid workspaceId)
                || workspaceId == Guid.Empty)
            {
                return Results.BadRequest(
                    new
                    {
                        code = "invalid_argument",
                        message = "A valid workspaceId route value is required."
                    });
            }

            ClaimsPrincipal user = context.HttpContext.User;
            string? issuer = user.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentityIssuerClaim);
            string? subject = user.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentitySubjectClaim);
            DomainResult result =
                await accessPolicy.AuthorizeWorkspaceGrantAsync(
                    workspaceId,
                    issuer ?? string.Empty,
                    subject ?? string.Empty,
                    context.HttpContext.RequestAborted);
            if (result.IsFailure)
            {
                return WebConsoleResults.Forbidden(
                    "This browser session cannot access the requested workspace.");
            }

            return await next(context);
        }
    }
}