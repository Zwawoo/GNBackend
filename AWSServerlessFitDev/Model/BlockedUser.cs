using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class BlockedUser
    {
        public string UserName { get; set; }
        public Guid UserSubId { get; set; }
        public string BlockedUserName { get; set; }
        public Guid BlockedUserSubId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }

    }
}
