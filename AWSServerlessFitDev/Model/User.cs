using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class User
    {
        public string UserName { get; set; }
        public Guid SubId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureLocalUrI { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string ProfilePictureHighResUrl { get; set; }
        public byte[] ProfilePicture { get; set; }
        public string Profile { get; set; }
        public string WebsiteString { get; set; }
        public string InstaString { get; set; }
        public int FollowsCount { get; set; }
        public int FollowerCount { get; set; }
        public bool IsAboCountHidden { get; set; }
        public bool IsPrivate { get; set; }
        public Gym PrimaryGym { get; set; }
        public Gym SecondaryGym { get; set; }
        public bool IsDeactivated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
    }
}
