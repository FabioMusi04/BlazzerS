using Microsoft.AspNetCore.Mvc;
using ReportingPortalServer.Services;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new { status = "API is running." });
        }
    }
}
