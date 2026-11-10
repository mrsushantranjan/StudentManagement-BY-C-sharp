using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Controllers;

[ApiController]
[Route("api/health")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Check()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "StudentManagement REST API",
            timestampUtc = DateTime.UtcNow
        });
    }
}

