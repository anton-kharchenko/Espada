using Espada.Api.Contracts.Responses;
using Espada.Api.Controllers;
using Espada.Domain.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Tests.Api.Controllers;

public sealed class BaseControllerTests
{
    public static TheoryData<string, int> KnownErrorStatusCodes =>
        new()
        {
            { "Workspace.NotFound", StatusCodes.Status404NotFound },
            { "Security.Unauthorized", StatusCodes.Status401Unauthorized },
            { "Security.Forbidden", StatusCodes.Status403Forbidden },
            { "Artifact.AlreadyArchived", StatusCodes.Status409Conflict },
            { "ArtifactRevision.Artifact.Archived", StatusCodes.Status409Conflict },
            { "Request.RateLimitExceeded", StatusCodes.Status429TooManyRequests },
            { "Source.Invalid", StatusCodes.Status400BadRequest }
        };

    [Theory]
    [MemberData(nameof(KnownErrorStatusCodes))]
    public void HandleError_WithKnownErrorCode_ShouldReturnExpectedStatusCode(string code, int expectedStatusCode)
    {
        DomainError error = new(code, "Test error description.");
        TestController controller = new();

        IActionResult result = controller.ExecuteHandleError(error);

        ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        ErrorResponse response = Assert.IsType<ErrorResponse>(objectResult.Value);

        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Equal(error.Code, response.Code);
        Assert.Equal(error.Description, response.Description);
    }

    private sealed class TestController : BaseController
    {
        public IActionResult ExecuteHandleError(DomainError error)
        {
            return HandleError(error);
        }
    }
}