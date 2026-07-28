using Espada.Domain.Rules;
using Espada.Protocol.Mcp.Constants;
using ModelContextProtocol;

namespace Espada.Protocol.Mcp.Services
{
    internal static class McpErrorMapper
    {
        public static void ThrowIfFailure(DomainResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            throw Create(result.Error);
        }

        public static McpException InvalidArgument(string description)
        {
            return new McpException($"{McpErrorCodeConstants.InvalidArgument}: {description}");
        }

        public static McpException Unauthorized(string description)
        {
            return new McpException($"{McpErrorCodeConstants.Unauthorized}: {description}");
        }

        public static McpException NotFound(string description)
        {
            return new McpException($"{McpErrorCodeConstants.NotFound}: {description}");
        }

        private static McpException Create(DomainError error)
        {
            return new McpException($"{Classify(error.Code)}: {error.Description}");
        }

        private static string Classify(string code)
        {
            if (code.Equals(
                    "Context.Budget.TooSmall",
                    StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.ContextBudgetTooSmall;
            }

            if (code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.Unauthorized;
            }

            if (code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
                || code.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.Forbidden;
            }

            if (code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase)
                || code.Contains(".NotFoundIn", StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.NotFound;
            }

            if (code.Contains("RateLimit", StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.RateLimited;
            }

            if (code.Contains(".Already", StringComparison.OrdinalIgnoreCase)
                || code.Contains(".Conflict", StringComparison.OrdinalIgnoreCase)
                || code.Contains(".Cannot", StringComparison.OrdinalIgnoreCase)
                || code.Contains(
                    "ArchivedCannot",
                    StringComparison.OrdinalIgnoreCase)
                || code.EndsWith(".Archived", StringComparison.OrdinalIgnoreCase))
            {
                return McpErrorCodeConstants.Conflict;
            }

            return McpErrorCodeConstants.InvalidArgument;
        }
    }
}