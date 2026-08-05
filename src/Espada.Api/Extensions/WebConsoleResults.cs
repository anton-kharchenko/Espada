using Espada.Domain.Rules;

namespace Espada.Api.Extensions
{
    internal static class WebConsoleResults
    {
        public static IResult From<TValue>(
            DomainResult<TValue> result,
            int successStatusCode = StatusCodes.Status200OK)
        {
            return result.IsFailure
                ? FromError(result.Error)
                : Results.Json(
                    result.Value,
                    statusCode: successStatusCode);
        }

        public static IResult From(DomainResult result)
        {
            return result.IsFailure
                ? FromError(result.Error)
                : Results.NoContent();
        }

        public static IResult Forbidden(string message)
        {
            return Results.Json(
                new
                {
                    code = "forbidden",
                    message
                },
                statusCode: StatusCodes.Status403Forbidden);
        }

        private static IResult FromError(DomainError error)
        {
            int statusCode = error.Code switch
            {
                "Access.Unauthorized" =>
                    StatusCodes.Status401Unauthorized,
                _ when error.Code.StartsWith(
                    "Access.Forbidden",
                    StringComparison.Ordinal) =>
                    StatusCodes.Status403Forbidden,
                _ when error.Code.Contains(
                    "NotFound",
                    StringComparison.Ordinal) =>
                    StatusCodes.Status404NotFound,
                _ when error.Code.Contains(
                           "Conflict",
                           StringComparison.Ordinal)
                       || error.Code.Contains(
                           "Duplicate",
                           StringComparison.Ordinal)
                       || error.Code.Contains(
                           "Already",
                           StringComparison.Ordinal) =>
                    StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return Results.Json(
                new
                {
                    code = error.Code,
                    message = error.Description
                },
                statusCode: statusCode);
        }
    }
}