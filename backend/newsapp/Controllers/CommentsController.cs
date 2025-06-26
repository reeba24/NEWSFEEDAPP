using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;
using NewsApp.Repository.Models;
using System.Data.SqlClient;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentsRepository _commentsRepo;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentsRepository commentsRepo, ILogger<CommentsController> logger)
        {
            _commentsRepo = commentsRepo;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] NewsComment comment)
        {
            try
            {
                var (newId, comments) = await _commentsRepo.AddCommentAsync(comment);

                return Ok(new
                {
                    message = "Comment added successfully",
                    comment_id = newId,
                    comments = comments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add comment");
                return StatusCode(500, "An error occurred while adding the comment.");
            }
        }

        [HttpGet("news/{news_id}")]
        public async Task<IActionResult> GetCommentsByNewsId(int news_id)
        {
            try
            {
                var comments = await _commentsRepo.GetCommentsByNewsIdAsync(news_id);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch comments for news {NewsId}", news_id);
                return StatusCode(500, "An error occurred while fetching comments.");
            }
        }
    }
}
