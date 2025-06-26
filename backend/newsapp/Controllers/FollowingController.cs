using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowingController : ControllerBase
    {
        private readonly IFollowingRepository _followingRepo;

        public FollowingController(IFollowingRepository followingRepo)
        {
            _followingRepo = followingRepo;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFollowedNews(int userId)
        {
            var result = await _followingRepo.GetFollowedNewsAsync(userId);
            return Ok(result);
        }
    }
}
