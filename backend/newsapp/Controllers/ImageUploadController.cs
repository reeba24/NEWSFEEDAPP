using Microsoft.AspNetCore.Mvc;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ImageUploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("upload")]
        public IActionResult UploadImages()
        {
            try
            {
                string connStr = _configuration.GetConnectionString("NewsDbConnection");
                ImageUpload.UploadAllImages(connStr);
                return Ok("Images uploaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, "Error: " + ex.Message);
            }
        }
    }
}
