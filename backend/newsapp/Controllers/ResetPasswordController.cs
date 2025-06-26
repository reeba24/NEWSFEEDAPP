using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IResetPasswordRepository _repository;

        public ResetPasswordController(IResetPasswordRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.newPassword))
            {
                return BadRequest(new { message = "Email and new password are required." });
            }

            string email = request.email.Trim();

            try
            {
                bool result = await _repository.ResetPasswordAsync(email, request.newPassword);
                if (!result)
                {
                    return NotFound(new { message = "User not found or inactive." });
                }

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }
    }
}
