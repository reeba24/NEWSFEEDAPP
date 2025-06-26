using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using newsapp.Models;
using NewsApp.Repository.Interfaces;

namespace newsapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISignUpRepository _signUpRepository;

        public SignUpController(IConfiguration configuration, ISignUpRepository signuprepository)
        {
            _configuration = configuration;
            _signUpRepository = signuprepository;
        }

        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            return Ok(new { message = "Connection successful!" });
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] Signup signup)
        {
            if (signup == null)
                return BadRequest("Invalid signup data.");

            signup.active = 1;

            string result = _signUpRepository.CreateSignUp(signup);

            if (result == "Data inserted")
                return Ok(new { message = "User registered successfully!" });
            else
                return BadRequest(new { message = result });
        }

        [HttpPost]
        [Route("login")]
        public string login([FromBody] login login)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection").ToString());

            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM USERS WHERE email=@Email AND active=1", con);
            da.SelectCommand.Parameters.AddWithValue("@Email", login.email);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                string storedHash = dt.Rows[0]["password"].ToString();

                var hasher = new PasswordHasher<login>();
                var result = hasher.VerifyHashedPassword(login, storedHash, login.password);

                if (result == PasswordVerificationResult.Success)
                {

                    int u_id = Convert.ToInt32(dt.Rows[0]["u_id"]);
                    SqlCommand cmd = new SqlCommand("UPDATE USERS SET signed_in = 1 WHERE u_id = @u_id", con);
                    cmd.Parameters.AddWithValue("@u_id", u_id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    return "data found";
                }
                else
                {
                    return "invalid user";
                }
            }
            else
            {
                return "invalid user";
            }
        }
    }
    }
