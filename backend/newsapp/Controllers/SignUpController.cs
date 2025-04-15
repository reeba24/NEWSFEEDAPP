using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsapp.Models;
using System.Data;
using NewsApp.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;

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

        [HttpGet("api/test")]
        public IActionResult TestConnection()
        {
            return Ok(new { message = "Connection successful!" });
        }

        [HttpPost]
        [Route("signup")]
        public IActionResult signup([FromBody] Signup signup)
        {
            if (signup == null)
            {
                return BadRequest("Invalid signup data.");
            }
            string result = _signUpRepository.CreateSignUp(signup);

            if (result == "Data inserted")  
            {
                return Ok(new { message = "User registered successfully!" });
            }
            else
            {
                return BadRequest(new { message = result });
            }
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