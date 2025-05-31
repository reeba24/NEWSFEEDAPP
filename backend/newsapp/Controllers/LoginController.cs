using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using newsapp.Models;
using System.Data.SqlClient;
using System.Data;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] login login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            string normalizedEmail = login.email.Trim();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection")))
            {
                string query = "SELECT password FROM USERS WHERE email = @Email AND active = 1";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", normalizedEmail);

                con.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedHash = reader["password"].ToString();

                    var hasher = new PasswordHasher<login>();
                    var result = hasher.VerifyHashedPassword(login, storedHash, login.password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        return Ok(new { message = "Login successful" });
                    }
                    else
                    {
                        return Unauthorized(new { message = "Invalid password." });
                    }
                }
                else
                {
                    return Unauthorized(new { message = "User not found or inactive." });
                }
            }
        }
    }
}
