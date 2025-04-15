using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using newsapp.Models;
using NewsApp.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace NewsApp.Repository
{
    public class SignUpRepository : ISignUpRepository
    {
        private readonly IConfiguration _configuration;
        public SignUpRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateSignUp(Signup signup)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("NewsDbConnection").ToString());

            // Step 1: Get the current max u_id value from the database (start from 1 if no records exist)
            SqlCommand getMaxIdCmd = new SqlCommand("SELECT MAX(u_id) FROM USERS", con);
            con.Open();
            var maxId = getMaxIdCmd.ExecuteScalar();
            int newUId = (maxId != DBNull.Value) ? Convert.ToInt32(maxId) + 1 : 1;  // If no max value found, start from 1

            // Step 2: Hash the password
            var hasher = new PasswordHasher<Signup>();
            string hashedPassword = hasher.HashPassword(signup, signup.password);
            signup.password = hashedPassword;

            // Step 3: Format dates (Ensure dates are within valid SQL range)
            string createdFormatted = (signup.created == DateTime.MinValue)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : signup.created.ToString("yyyy-MM-dd HH:mm:ss");

            string modifiedFormatted = (signup.modified_time == DateTime.MinValue)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : signup.modified_time.ToString("yyyy-MM-dd HH:mm:ss");

            // Step 4: Assign the same value to user_role_key (same as u_id)
            signup.user_role_key = newUId;

            // Step 5: Create the SQL command to insert the new user
            SqlCommand cmd = new SqlCommand(
                "INSERT INTO USERS (" +
                "u_id, email, password, first_name, last_name, email_verified, active, created, " +
                "created_by, about, user_role_key, modified_time) " +
                "VALUES (" +
                newUId + ", " + // Insert the incremented u_id
                "'" + signup.email + "', " +
                "'" + signup.password + "', " +
                "'" + signup.first_name + "', " +
                "'" + signup.last_name + "', " +
                "'" + signup.email_verified + "', " +
                "'" + signup.active + "', " +
                "'" + createdFormatted + "', " +
                "'" + signup.created_by + "', " +
                "'" + signup.about + "', " +
                newUId + ", " + // Assign the same value to user_role_key
                "'" + modifiedFormatted + "')", con);

            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                return "Data inserted successfully with new u_id.";
            }
            else
            {
                return "Error inserting data.";
            }
        }
    }
}
