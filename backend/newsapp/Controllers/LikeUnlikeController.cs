using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeUnlikeController : ControllerBase
    {
        private readonly ILikeUnlikeRepository _repository;

        public LikeUnlikeController(ILikeUnlikeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("action")]
        public async Task<IActionResult> PerformLikeAction([FromBody] Like model)
        {
            try
            {
                var message = await _repository.PerformLikeActionAsync(model);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("fullstatus")]
        public async Task<IActionResult> GetFullStatus([FromQuery] int news_id, [FromQuery] int u_id)
        {
            try
            {
                var result = await _repository.GetFullStatusAsync(news_id, u_id);
                return Ok(new
                {
                    hasLiked = result.hasLiked,
                    hasUnliked = result.hasUnliked,
                    hasSaved = result.hasSaved
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
