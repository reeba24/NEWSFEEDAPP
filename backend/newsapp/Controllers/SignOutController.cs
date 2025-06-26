using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignOutController : ControllerBase
    {
        private readonly ISignOutRepository _repository;

        public SignOutController(ISignOutRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> SignOut([FromBody] Signout request)
        {
            if (request == null || request.u_id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            bool result = await _repository.SignOutUserAsync(request.u_id);

            if (result)
                return Ok(new { message = "User signed out successfully." });
            else
                return NotFound(new { message = "User not found." });
        }
    }
}
