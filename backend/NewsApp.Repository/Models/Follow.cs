using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Repository.Models
{
    public class Follow
    {
        public int FollowedUid { get; set; }
        public int FollowedByUid { get; set; }
    }
}
