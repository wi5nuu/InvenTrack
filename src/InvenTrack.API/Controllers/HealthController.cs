using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            application = "InvenTrack Enterprise API"
        });
    }
}
