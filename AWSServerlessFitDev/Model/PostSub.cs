using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class PostSub
    {
        public string UserName { get; set; }
        public long PostId { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
