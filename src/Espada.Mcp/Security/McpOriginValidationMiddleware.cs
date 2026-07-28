using Microsoft.Extensions.Options;

namespace Espada.Mcp.Security
{
    internal sealed class McpOriginValidationMiddleware(
        RequestDelegate next,
        IOptions<McpAuthorizationOptions> options)
    {
        private readonly HashSet<string> _allowedOrigins = options.Value
            .AllowedOrigins
            .Select(NormalizeOrigin)
            .Where(origin => origin is not null)
            .Select(origin => origin!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public async Task InvokeAsync(HttpContext context)
        {
            string? originValue = context.Request.Headers.Origin.FirstOrDefault();
            if (originValue is not null
                && !IsAllowed(context.Request, originValue))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await next(context);
        }

        private bool IsAllowed(HttpRequest request, string originValue)
        {
            string? origin = NormalizeOrigin(originValue);
            if (origin is null)
            {
                return false;
            }

            string requestOrigin =
                $"{request.Scheme}://{request.Host.Value}".TrimEnd('/');
            return origin.Equals(
                       requestOrigin,
                       StringComparison.OrdinalIgnoreCase)
                   || _allowedOrigins.Contains(origin);
        }

        private static string? NormalizeOrigin(string value)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? origin)
                || (!string.IsNullOrEmpty(origin.PathAndQuery)
                    && origin.PathAndQuery != "/")
                || !string.IsNullOrEmpty(origin.Fragment)
                || origin.Scheme is not ("http" or "https"))
            {
                return null;
            }

            return origin.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }
    }
}