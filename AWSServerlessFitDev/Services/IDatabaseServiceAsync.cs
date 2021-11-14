using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.Chat;

namespace AWSServerlessFitDev.Services
{

    public interface IDatabaseServiceAsync
    {
        Task<bool> GetUserHasCreatedProfile(string username);
        Task SetUserHasCreatedProfile(string username);
        Task EditUserProfile(User user);
        Task<User> GetUser(string username, bool IsOwnProfile);
        Task<IEnumerable<User>> GetUsersByUserNameOrFullName(string searchString);
        Task<IEnumerable<Gym>> GetGyms(string cityName, string gymName, int maxCount);
        Task<IEnumerable<Gym>> SearchGyms(long lastGroupId, string searchText, double? leastRelevance, int limit);
        Task<Group> GetGroup(int groupId);
        Task UserSetPrimaryGym(string userName, int gymId);
        Task UserRemovePrimaryGym(string userName);
        Task SetOrUpdateGroupMember(int groupId, string userName, UserGroupRole role);
        Task<long?> InsertPost(Post post);
        Task UpdatePost(long postId, string description);
        Task<IEnumerable<Post>> GetPostsFromOwnUser(string userName);
        Task<IEnumerable<Post>> GetPostsFromForeignUser(string userName);
        Task<IEnumerable<Post>> GetGroupPosts(int groupId, long startOffsetPostId, int limit);
        Task<IEnumerable<Post>> GetGroupPosts(int groupId, long startOffsetPostId, string searchText, double? leastRelevance, int limit);
        Task<IEnumerable<Post>> GetNewsfeedPosts(string userName, long startOffsetPostId, int limit);
        Task<Post> GetPost(long postId);
        Task DeletePostWithFlag(long postId);
        Task InsertOrReplacePostLikeIfNewer(PostLike postLike);
        Task InsertOrReplacePostSubIfNewer(PostSub postSub);
        Task<IEnumerable<PostLike>> GetAllPostLikesFromUserSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<PostSub>> GetAllPostSubsFromUserSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<User>> GetPostSubbedBy(long postId);
        Task<bool> IsUserSubbedToPost(string userName, long postId);
        Task<int> InsertOrReplaceFollowIfNewer(Follow follow);
        Task<IEnumerable<Follow>> GetAllFollowsFromUserSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<Follow>> GetAllFollowersFromUserSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<Follow>> GetPendingFollowersFromUser(string userName);
        Task<bool> IsUser1FollowingUser2(string userName1, string userName2);
        Task InsertUserDeviceEndpoint(string userName, string endpointArn, string deviceType, string deviceToken);
        Task DeleteUserDeviceEndpoint(string userName, string deviceToken);
        Task<IEnumerable<Device>> GetUserDevices(string userName);
        Task<int> UpdateFollowToAccepted(string follower, string following);
        Task UpdateFollowToDenied(string follower, string following);
        long? InsertNotification(Notification notification);
        IEnumerable<PostComment> GetPostComments(long postId);
        Task<long?> InsertPostComment(PostComment postComment);
        Task<PostComment> GetPostComment(long postCommentId);
        Task DeletePostCommentWithFlag(long postCommentId);
        Task<IEnumerable<User>> GetPostLikedBy(long postId);
        Task<IEnumerable<User>> GetUserFollowedByUser(string userName, string offsetOldestUserName, int limit);
        Task<IEnumerable<User>> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit);
        Task<IEnumerable<User>> GetGroupMembers(int groupId, string searchText, string offsetOldestUserName, int limit);
        /*
        * Creates a direct COnversation if not existing. Returns the existing or new created ConversationId for the 2 users.
        */
        Task<long?> CreateDirectConversationIfNotExist(string userName, string toUserName);
        Task<IEnumerable<Conversation>> GetNewOrUpdatedConversations(string userName, DateTime lastSyncTime);
        Task<IEnumerable<Conversation_Participant>> GetConversationParticipants(long conversationId);
        Task UpdateConversationParticipantIfNewer(Conversation_Participant cP);
        Task<int> InsertOrIgnoreChatMessage(ChatMessage chatMessage);
        Task<ChatMessage> GetChatMessage(Guid messageId);
        Task<IEnumerable<Conversation_Participant>> GetNewOrUpdatedConversationParticipants(string userName, DateTime lastSync);
        //[Obsolete]
        //IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync, bool withAttachments);
        Task<IEnumerable<ChatMessage>> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync);
        Task InsertOrIgnoreAttachment(ChatMessage_Attachment attachment);
        Task<ChatMessage_Attachment> GetChatMessageAttachment(Guid attachmentId);
        Task<int> InsertOrIgnoreChatMessageWithAttachments(ChatMessage chatMessage);
        Task<IEnumerable<Notification>> GetNotifications(string userName, DateTime sinceDateTime);
        Task<int> GetGroupMemberCount(int groupId);
        Task DeleteNotifications(string from, string to, NotificationType notificationType, long postId = -1);
        Task InsertFeedback(string userName, string subject, string text);
        Task InsertOrReplaceBlockedUserIfNewer(BlockedUser blockedUser);
        Task<IEnumerable<BlockedUser>> GetAllBlockedUsersFromUserSinceDate(string userName, DateTime sinceDate);
        Task<bool> IsUser1BlockedByUser2(string blockedUserName1, string userName2);
        Task<IEnumerable<BlockedUser>> GetBlockingUsersFor(string userName);
        Task RemoveAllPostSubsFromUser1OnUser2(string blockedUserName1, string userName2);
    }


}
