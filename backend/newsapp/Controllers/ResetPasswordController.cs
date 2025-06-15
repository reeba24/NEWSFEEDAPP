using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using NewsApp.Repository.Models;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ResetPasswordController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.newPassword))
            {
                return BadRequest(new { message = "Email and new password are required." });
            }

            string email = request.email.Trim();

            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
                {
                    con.Open();

                    string checkUserQuery = "SELECT * FROM USERS WHERE email = @Email AND active = 1";
                    using (SqlCommand checkCmd = new SqlCommand(checkUserQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", email);

                        using (var reader = checkCmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return NotFound(new { message = "User not found or inactive." });
                            }
                        }
                    }

                    var hasher = new PasswordHasher<ResetPasswordRequest>();
                    string hashedPassword = hasher.HashPassword(request, request.newPassword);

                    string updateQuery = "UPDATE USERS SET password = @Password WHERE email = @Email AND active = 1";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        updateCmd.Parameters.AddWithValue("@Email", email);

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "Password reset successfully." });
                        }
                        else
                        {
                            return StatusCode(500, new { message = "Failed to reset password." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }
    }
}
