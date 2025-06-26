using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchRepository _repository;

        public SearchController(ISearchRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{keyword}")]
        public async Task<IActionResult> SearchNews(string keyword)
        {
            var result = await _repository.SearchNewsAsync(keyword);
            return Ok(result);
        }
    }
}
