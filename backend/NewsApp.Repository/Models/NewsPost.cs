using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NewsApp.Repository.Models
{
    public class NewsPost
    {
        public string news_title { get; set; }
        public string contents { get; set; }
        public int u_id { get; set; }
        public int active { get; set; }
         public string pref_name { get; set; }
        public IFormFile image { get; set; }
    }
}
