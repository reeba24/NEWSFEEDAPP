using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
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
        public int u_id { get; set; }
        public bool isFollowed { get; set; }
        public bool hasLiked { get; set; }
        public bool hasUnliked { get; set; }
        public bool hasSaved { get; set; }
        public int read_time { get; set; }
        public List<CommentModel> comments { get; set; } = new List<CommentModel>();
    }

    public class CommentModel
    {
        public int comment_id { get; set; }
        public int news_id { get; set; }
        public int u_id { get; set; }
        public string comments { get; set; }
        public DateTime created_time { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
}
