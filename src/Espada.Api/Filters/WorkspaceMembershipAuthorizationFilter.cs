using Espada.Application.Contracts.Persistence;
using Espada.Domain.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Espada.Api.Filters;

internal sealed class WorkspaceMembershipAuthorizationFilter(IWorkspaceMembershipRepository membershipRepository) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.AuthenticationType != JwtBearerDefaults.AuthenticationScheme
            || !context.RouteData.Values.TryGetValue("workspaceId", out object? routeValue)
            || !Guid.TryParse(routeValue?.ToString(), out Guid workspaceId)
            || workspaceId == Guid.Empty)
        {
            return;
        }

        string? issuer = context.HttpContext.User.FindFirst("iss")?.Value;
        string? subject = context.HttpContext.User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(issuer)
            || string.IsNullOrWhiteSpace(subject)
            || !await membershipRepository.IsMemberAsync(WorkspaceId.Create(workspaceId), issuer, subject, context.HttpContext.RequestAborted))
        {
            context.Result = new ForbidResult();
        }
    }
}