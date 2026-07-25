using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Controllers;

[Route("api/v{version:apiVersion}/system")]
public sealed class SystemController : BaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "Espada.Api",
            status = "running",
            utcNow = DateTimeOffset.UtcNow
        });
    }
}