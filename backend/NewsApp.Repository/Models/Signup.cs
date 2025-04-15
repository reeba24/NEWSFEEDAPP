namespace newsapp.Models
{
    public class Signup
    {
        public int u_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }
        public int email_verified { get; set; }
        public int active { get; set; }
        public DateTime created { get; set; }
        public string created_by { get; set; }
        public string about { get; set; }
        public int user_role_key { get; set; }
        public DateTime modified_time { get; set; }
    }
}
