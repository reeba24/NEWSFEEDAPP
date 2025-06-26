using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Repository.Models
{
    public class Like
    {
        public int u_id { get; set; }
        public int news_id { get; set; }
        public string action { get; set; } = "like";
    }
}
