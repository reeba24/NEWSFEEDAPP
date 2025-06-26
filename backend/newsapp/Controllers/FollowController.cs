using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowRepository _followRepo;
        private readonly ILogger<FollowController> _logger;

        public FollowController(IFollowRepository followRepo, ILogger<FollowController> logger)
        {
            _followRepo = followRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> FollowUser([FromBody] Follow follow)
        {
            if (follow.FollowedByUid <= 0 || follow.FollowedUid <= 0)
                return BadRequest(new { message = "Invalid user ID(s)." });

            try
            {
                var message = await _followRepo.ToggleFollowAsync(follow);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following/unfollowing user.");
                return StatusCode(500, new { message = "Server error." });
            }
        }
    }
}
