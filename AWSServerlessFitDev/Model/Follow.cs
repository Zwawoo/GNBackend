using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Follow
    {
        public string Follower { get; set; }
        public string Following { get; set; }
        public bool IsPending { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
