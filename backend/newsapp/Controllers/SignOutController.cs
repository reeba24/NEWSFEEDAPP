using Microsoft.AspNetCore.Mvc;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignOutController : ControllerBase
    {
        [HttpPost]
        public IActionResult SignOut()
        {
            return Ok(new { message = "User signed out successfully." });
        }
    }
}
