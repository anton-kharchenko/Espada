using System.Net;
using Microsoft.Extensions.Primitives;

namespace Espada.Api.Authentication
{
    internal static class WebConsoleRequestSecurity
    {
        public static bool IsAllowed(
            HttpContext context,
            WebConsoleOptions options)
        {
            return options.Mode == WebConsoleMode.Cloud
                   || IsLoopback(context.Connection.RemoteIpAddress)
                   && IsLoopbackHost(context.Request.Host.Host);
        }

        public static bool HasSameOrigin(HttpRequest request)
        {
            if (!request.Headers.TryGetValue(
                    "Origin",
                    out StringValues originValues)
                || originValues.Count != 1
                || !Uri.TryCreate(
                    originValues[0],
                    UriKind.Absolute,
                    out Uri? origin))
            {
                return false;
            }

            return origin.Scheme.Equals(
                       request.Scheme,
                       StringComparison.OrdinalIgnoreCase)
                   && origin.Host.Equals(
                       request.Host.Host,
                       StringComparison.OrdinalIgnoreCase)
                   && origin.Port == ResolveRequestPort(request);
        }

        public static string ValidateLocalReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return "/app";
            }

            return returnUrl.StartsWith("/", StringComparison.Ordinal)
                   && !returnUrl.StartsWith("//", StringComparison.Ordinal)
                   && !returnUrl.StartsWith("/\\", StringComparison.Ordinal)
                ? returnUrl
                : throw new BadHttpRequestException(
                    "returnUrl must be a local absolute path.");
        }

        private static bool IsLoopback(IPAddress? address)
        {
            return address is not null && IPAddress.IsLoopback(address);
        }

        private static bool IsLoopbackHost(string host)
        {
            return host.Equals(
                       "localhost",
                       StringComparison.OrdinalIgnoreCase)
                   || IPAddress.TryParse(host, out IPAddress? address)
                   && IPAddress.IsLoopback(address);
        }

        private static int ResolveRequestPort(HttpRequest request)
        {
            return request.Host.Port
                   ?? (request.Scheme.Equals(
                       Uri.UriSchemeHttps,
                       StringComparison.OrdinalIgnoreCase)
                       ? 443
                       : 80);
        }
    }
}
