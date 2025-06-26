using Microsoft.AspNetCore.Mvc;
using newsapp.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepo;

        public LoginController(ILoginRepository loginRepo)
        {
            _loginRepo = loginRepo;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] login login)
        {
            var (success, userId, message) = await _loginRepo.SignInAsync(login);

            if (success)
                return Ok(new { u_id = userId, message });
            else if (message == "Invalid password." || message == "User not found or inactive.")
                return Unauthorized(new { message });
            else
                return BadRequest(new { message });
        }
    }
}
