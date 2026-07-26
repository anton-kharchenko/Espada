using Microsoft.AspNetCore.Authorization;
using Espada.Api.Security;
using Asp.Versioning;
using Espada.Api.Contracts.Responses;
using Espada.Domain.Rules;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
[ApiController]
[ApiVersion("1.0")]
[ApiConventionType(typeof(DefaultApiConventions))]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public abstract class BaseController : ControllerBase
{
    internal BadRequestObjectResult BadRequest(DomainError error)
    {
        return BadRequest(new ErrorResponse(error.Code, error.Description));
    }

    public IActionResult HandleError(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        ErrorResponse errorResponse = new(error.Code, error.Description);

        return error.Code switch
        {
            var code when IsNotFound(code) => NotFound(errorResponse),
            var code when IsUnauthorized(code) => Unauthorized(errorResponse),
            var code when IsForbidden(code) => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
            var code when IsConflict(code) => Conflict(errorResponse),
            var code when IsRateLimit(code) => StatusCode(StatusCodes.Status429TooManyRequests, errorResponse),
            _ => BadRequest(errorResponse)
        };
    }

    private static bool IsNotFound(string code) => code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase) || code.Contains(".NotFoundIn", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnauthorized(string code) => code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase);

    private static bool IsForbidden(string code) => code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) || code.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase);

    private static bool IsConflict(string code) => code.Contains(".Already", StringComparison.OrdinalIgnoreCase) || code.Contains(".Conflict", StringComparison.OrdinalIgnoreCase) || code.Contains(".Cannot", StringComparison.OrdinalIgnoreCase) || code.Contains("ArchivedCannot", StringComparison.OrdinalIgnoreCase);

    private static bool IsRateLimit(string code) => code.Contains("RateLimit", StringComparison.OrdinalIgnoreCase);
}