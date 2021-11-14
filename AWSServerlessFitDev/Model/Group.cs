using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Group
    {
        public Group()
        {
        }
        public Group(int groupId, string groupName, string description, int? creator, GroupPrivacyTypes groupPrivacyType, DateTime? createdAt, DateTime? lastModified)
        {
            GroupId = groupId;
            GroupName = groupName;
            Description = description;
            Creator = creator;
            PrivacyType = groupPrivacyType;
            CreatedAt = createdAt;
            LastModified = lastModified;

        }


        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int? Creator { get; set; }
        public GroupPrivacyTypes PrivacyType { get; set; }
        public bool IsGym { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
        public double SearchRelevance { get; set; }
        public int MemberCount { get; set; }
    }
}
