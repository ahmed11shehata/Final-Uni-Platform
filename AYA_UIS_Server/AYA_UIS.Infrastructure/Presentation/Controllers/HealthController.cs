using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// Simple health check so the frontend can confirm the API is running.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
