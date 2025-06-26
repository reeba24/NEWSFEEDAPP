using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;
using newsapp.Repositories;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewPostController : ControllerBase
    {
        private readonly INewPostRepository _newPostRepo;

        public NewPostController(INewPostRepository newPostRepo)
        {
            _newPostRepo = newPostRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] NewsPost post)
        {
            try
            {
                var message = await _newPostRepo.CreatePostAsync(post);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }
    }
}
