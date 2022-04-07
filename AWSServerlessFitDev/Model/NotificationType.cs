using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public enum NotificationType
    {
        None = -1,

        FollowRequest = 0,
        
        FollowAccepted = 1,
       
        PostLike = 2,

        PostComment = 3,

        PostLinking = 4,

        ChatMessage = 5,

        Follow = 6,

        Unfollow = 7,

        PostUnlike = 8,

        SubbedPostComment = 9,

        FollowRemoved = 10,

        CommentLinking = 11,

        GymPostPublished = 12

    }
}
