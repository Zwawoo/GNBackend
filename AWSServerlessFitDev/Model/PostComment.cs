using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class PostComment
    {
        public long? Id { get; set; }
        public long? ServerId { get; set; }
        public long PostId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public int LikeCount { get; set; }
        public DateTime? TimePosted { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
        
    }
}
