using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedRepository _feedRepository;

        public FeedController(IFeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetFeedStatus([FromQuery] int userId)
        {
            var status = await _feedRepository.GetFeedStatusAsync(userId);

            if (status == "error")
                return StatusCode(500, new { error = "Something went wrong" });

            return Ok(new { status });
        }
    }
}

