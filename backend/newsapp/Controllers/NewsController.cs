using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsRepository _newsRepository;

        public NewsController(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews([FromQuery] int userId)
        {
            try
            {
                var newsList = await _newsRepository.GetAllNewsAsync(userId);
                return Ok(newsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }
    }
}
