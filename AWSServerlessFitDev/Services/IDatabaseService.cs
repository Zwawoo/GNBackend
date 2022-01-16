using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.Chat;
using AWSServerlessFitDev.Model.WorkoutModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public interface IDatabaseService
    {
        bool GetUserHasCreatedProfile(string username);
        void SetUserHasCreatedProfile(string username);
        void EditUserProfile(User user);
        User GetUser(string username, bool IsOwnProfile);
        User AdminGetUserOnly(string username);
        IEnumerable<User> GetUsersForClearing();
        void ClearUser(Guid subId);
        IEnumerable<User> GetUsersByUserNameOrFullName(string searchString, bool callerIsAdmin = false);
        IEnumerable<Gym> GetGyms(string cityName, string gymName, int maxCount);
        IEnumerable<Gym> SearchGyms(long lastGroupId, string searchText, double? leastRelevance, int limit);
        Group GetGroup(int groupId);
        void UserSetPrimaryGym(string userName, int gymId);
        void UserRemovePrimaryGym(string userName);
        void SetOrUpdateGroupMember(int groupId, string userName, UserGroupRole role);
        long? InsertPost(Post post);
        void UpdatePost(long postId, string description);
        void InsertReport(string authenticatedUserName, string reportedUser, long? reportedPost, long? reportedPostComment, string reason);
        User AdminGetUser(string userName);
        IEnumerable<Post> GetPostsFromOwnUser(string userName);
        IEnumerable<Post> GetPostsFromForeignUser(string userName, bool callerIsAdmin = false);
        IEnumerable<Post> GetAllPostsFromUser(Guid subId);
        IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, string searchText, double? leastRelevance, int limit, bool callerIsAdmin = false);
        IEnumerable<Post> GetNewsfeedPosts(string userName, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        Post GetPost(long postId);
        void DeletePostWithFlag(long postId);
        void InsertOrReplacePostLikeIfNewer(PostLike postLike);
        void InsertOrUpdatePostSubIfNewer(PostSub postSub);
        IEnumerable<PostLike> GetAllPostLikesFromUserSinceDate(string userName, DateTime sinceDate);
        IEnumerable<PostSub> GetAllPostSubsFromUserSinceDate(string userName, DateTime sinceDate);
        IEnumerable<User> GetPostSubbedBy(long postId);
        bool IsUserSubbedToPost(string userName, long postId);
        int InsertOrReplaceFollowIfNewer(Follow follow);
        IEnumerable<Follow> GetAllFollowsFromUserSinceDate(string userName, DateTime sinceDate);
        IEnumerable<Follow> GetAllFollowersFromUserSinceDate(string userName, DateTime sinceDate);
        IEnumerable<Follow> GetPendingFollowersFromUser(string userName);
        
        bool IsUser1FollowingUser2(string userName1, string userName2);
        void InsertUserDeviceEndpoint(string userName, string endpointArn, string deviceType, string deviceToken);
        void DeleteUserDeviceEndpoint(string userName, string deviceToken);
        IEnumerable<Device> GetUserDevices(string userName);
        void AdminSetUserDeactivatedStatus(string userName, bool isDeactivated);
        int UpdateFollowToAccepted(string follower, string following);
        void UpdateFollowToDenied(string follower, string following);
        long? InsertNotification(Notification notification);
        IEnumerable<PostComment> GetPostComments(long postId);
        long? InsertPostComment(PostComment postComment);
        
        PostComment GetPostComment(long postCommentId);
        void DeletePostCommentWithFlag(long postCommentId);
        IEnumerable<User> GetPostLikedBy(long postId);
        IEnumerable<User> GetUserFollowedByUser(string userName, string offsetOldestUserName, int limit);
        IEnumerable<User> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit);
        IEnumerable<User> GetGroupMembers(int groupId, string searchText, string offsetOldestUserName, int limit);
        void AdminSetPostDeactivatedStatus(long postId, bool isDeactivated);

        /*
* Creates a direct COnversation if not existing. Returns the existing or new created ConversationId for the 2 users.
*/
        long? CreateDirectConversationIfNotExist(string userName, string toUserName);
        
        IEnumerable<Conversation> GetNewOrUpdatedConversations(string userName, DateTime lastSyncTime);
        IEnumerable<Conversation_Participant> GetConversationParticipants(long conversationId);
        void UpdateConversationParticipantIfNewer(Conversation_Participant cP);
        int InsertOrIgnoreChatMessage(ChatMessage chatMessage);
        ChatMessage GetChatMessage(Guid messageId);
        IEnumerable<Conversation_Participant> GetNewOrUpdatedConversationParticipants(string userName, DateTime lastSync);
        void AdminSetReportHandled(long reportId, string userName, string actionTaken);

        //[Obsolete]
        //IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync, bool withAttachments);
        IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync);
        void InsertOrIgnoreAttachment(ChatMessage_Attachment attachment);
        ChatMessage_Attachment GetChatMessageAttachment(Guid attachmentId);
        int InsertOrIgnoreChatMessageWithAttachments(ChatMessage chatMessage);
        IEnumerable<Notification> GetNotifications(string userName, DateTime sinceDateTime);
        int GetGroupMemberCount(int groupId);
        void DeleteNotifications(string from, string to, NotificationType notificationType, long postId = -1);
        void InsertFeedback(string userName, string subject, string text);
        void InsertOrUpdateBlockedUserIfNewer(BlockedUser blockedUser);
        IEnumerable<BlockedUser> GetAllBlockedUsersFromUserSinceDate(string userName, DateTime sinceDate);
        bool IsUser1BlockedByUser2(string blockedUserName1, string userName2);
        IEnumerable<BlockedUser> GetBlockingUsersFor(string userName);
        void RemoveAllPostSubsFromUser1OnUser2(string blockedUserName1, string userName2);
        IEnumerable<Report> AdminGetReports(bool isHandled, long lastReportId, int limit);
        void DeleteUserWithFlag(string userName);

        void InsertOrUpdateWorkoutPlanIfNewer(WorkoutPlan wp);
        void InsertOrUpdateExerciseIfNewer(Exercise ex);
        void InsertOrUpdateWorkoutPlanExerciseIfNewer(string userName, WorkoutPlanExercise wpEx);
        IEnumerable<Exercise> GetAllExercisesSinceDate(string userName, DateTime sinceDate);
        IEnumerable<WorkoutPlan> GetAllWorkoutPlansSinceDate(string userName, DateTime sinceDate);
        IEnumerable<WorkoutPlanExercise> GetAllWorkoutPlanExercisesSinceDate(string userName, DateTime sinceDate);
        IEnumerable<Equipment> GetEquipmentSinceDate(DateTime sinceDate);
        IEnumerable<Muscle> GetMusclesSinceDate(DateTime sinceDate);
        void InsertOrUpdateWorkoutIfNewer(string userName, Workout w);
        IEnumerable<Workout> GetAllWorkoutsSinceDate(string userName, DateTime sinceDate);
        WorkoutPlanSyncData GetPublicWorkoutPlans(string userName);
        void CopyWorkoutPlan(Guid workoutPlanId, string userName);
    }
}
