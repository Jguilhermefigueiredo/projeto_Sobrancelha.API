using Microsoft.AspNetCore.Mvc;

namespace SombrancelhaApp.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "UP",
            timestamp = DateTime.UtcNow
        });
    }
}
