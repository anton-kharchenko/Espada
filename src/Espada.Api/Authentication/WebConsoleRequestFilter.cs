using Espada.Api.Authentication.Constants;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

namespace Espada.Api.Authentication
{
    internal sealed class WebConsoleRequestFilter(
        IOptions<WebConsoleOptions> options,
        IAntiforgery antiforgery) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            HttpContext httpContext = context.HttpContext;
            if (!WebConsoleRequestSecurity.IsAllowed(
                    httpContext,
                    options.Value))
            {
                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            if (HttpMethods.IsPost(httpContext.Request.Method)
                || HttpMethods.IsPut(httpContext.Request.Method)
                || HttpMethods.IsPatch(httpContext.Request.Method)
                || HttpMethods.IsDelete(httpContext.Request.Method))
            {
                if (!WebConsoleRequestSecurity.HasSameOrigin(
                        httpContext.Request))
                {
                    return Results.StatusCode(
                        StatusCodes.Status403Forbidden);
                }

                try
                {
                    await antiforgery.ValidateRequestAsync(httpContext);
                }
                catch (AntiforgeryValidationException)
                {
                    return Results.BadRequest(
                        new
                        {
                            code = "invalid_antiforgery_token",
                            message =
                                $"Provide the {WebConsoleAuthenticationConstants.AntiforgeryHeaderName} header."
                        });
                }
            }

            return await next(context);
        }
    }
}