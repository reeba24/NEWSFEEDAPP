using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditProfileController : ControllerBase
    {
        private readonly IEditProfileRepository _repository;

        public EditProfileController(IEditProfileRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("GetProfile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                var profile = await _repository.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateProfile([FromBody] EditProfile profile)
        {
            try
            {
                var updated = await _repository.UpdateProfileAsync(profile);
                return updated
                    ? Ok(new { message = "Profile updated successfully" })
                    : StatusCode(500, "Update failed");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
