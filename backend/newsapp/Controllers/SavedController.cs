using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedController : ControllerBase
    {
        private readonly ISavedRepository _repository;

        public SavedController(ISavedRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveNews([FromBody] Saved saved)
        {
            bool success = await _repository.SaveNewsAsync(saved);
            return success ? Ok(new { message = "Saved!" }) : StatusCode(500, "Failed to save news.");
        }

        [HttpPost("unsave")]
        public async Task<IActionResult> UnsaveNews([FromBody] Saved saved)
        {
            bool success = await _repository.UnsaveNewsAsync(saved);
            return success ? Ok(new { message = "Unsaved!" }) : NotFound("No saved record found to unsave.");
        }

        [HttpGet("getsavednews/{u_id}")]
        public async Task<IActionResult> GetSavedNews(int u_id)
        {
            var result = await _repository.GetSavedNewsAsync(u_id);
            return Ok(result);
        }
    }
}
