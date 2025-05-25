using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Repository.Models
{
    public class TileData
    {
        public int news_id { get; set; }
        public string news_title { get; set; }
        public byte[] image { get; set; }           
        public string? imageBase64 { get; set; }   
        public string contents { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }
        public int likes { get; set; }
        public int unlikes { get; set; }
        public DateTime created_time { get; set; }

    }
}
