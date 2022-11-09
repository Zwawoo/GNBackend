using System;

namespace AWSServerlessFitDev.Model
{
    public class PostCommentLike
    {
        public string UserName { get; set; }
        public long PostCommentId { get; set; }
        public long PostId { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
