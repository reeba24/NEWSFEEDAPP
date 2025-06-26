using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendingNewsController : ControllerBase
    {
        private readonly ITrendingNewsRepository _repository;

        public TrendingNewsController(ITrendingNewsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendingNews()
        {
            var trendingNews = await _repository.GetTrendingNewsAsync();
            return Ok(trendingNews);
        }
    }
}
