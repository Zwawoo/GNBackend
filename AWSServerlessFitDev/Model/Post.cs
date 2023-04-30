using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Post
    {
        public long? PostId { get; set; }
        public string UserName { get; set; }
        public PostType? PostType { get; set; }
        public string PostImageLocalUri { get; set; }
        public byte[] PostResource { get; set; }
        public string PostResourceUrl { get; set; }
        public byte[] PostResourceThumbnail { get; set; }
        public string PostResourceThumbnailUrl { get; set; }
        public bool IsProfilePost { get; set; }
        public int? GroupId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDeactivated { get; set; }
        public double SearchRelevance { get; set; }
        public bool IsSponsored { get; set; }
        public string AffiliateLink { get; set; }
        public int? BrandId { get; set; }
        public PostVisibility? PostVisibility { get; set; }
    }
}
