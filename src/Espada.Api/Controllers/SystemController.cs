using Espada.Api.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/system")]
public sealed class SystemController : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SystemResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new SystemResponse(Service: "Espada.Api", Status: "running", UtcNow: DateTimeOffset.UtcNow));
    }
}
