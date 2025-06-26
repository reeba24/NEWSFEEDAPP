using Microsoft.AspNetCore.Mvc;
using newsapp.Repositories;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repository;

        public NotificationController(INotificationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("GetNotifications/{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var result = await _repository.GetNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpPost("MarkAsRead/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _repository.MarkAsReadAsync(notificationId);
            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPost("ClearAll/{userId}")]
        public async Task<IActionResult> ClearAll(int userId)
        {
            await _repository.ClearAllAsync(userId);
            return Ok(new { message = "All notifications cleared" });
        }
    }
}
