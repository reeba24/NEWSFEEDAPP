using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Repository.Models
{
    public class NotificationTypeMaster
    {
        public int notificationtype_id { get; set; }
        public string notificationtype_text { get; set; }
        public string notification_dummy_text { get; set; }
        public bool active { get; set; }
    }
}
