using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Repository.Models
{
    public class Notification
    {
        public int notification_id { get; set; }
        public int u_id { get; set; }
        public int notificationtype_id { get; set; }
        public DateTime created_time { get; set; }
        public DateTime? read_time { get; set; }
        public string notification_text { get; set; }
        public bool active { get; set; }
    }
}
