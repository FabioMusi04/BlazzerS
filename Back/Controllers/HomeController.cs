using Microsoft.AspNetCore.Mvc;
using Back.Services;

namespace Back.Controllers
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
