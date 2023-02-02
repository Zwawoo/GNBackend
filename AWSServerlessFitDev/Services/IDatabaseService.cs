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
        //        bool GetUserHasCreatedProfile(string username);
        //        void SetUserHasCreatedProfile(string username);
        //        void EditUserProfile(User user);
        //        User GetUser(string username, bool IsOwnProfile);
        //        User AdminGetUserOnly(string username);
        //        IEnumerable<User> GetUsersForClearing();
        //        void ClearUser(Guid subId);
        //        IEnumerable<User> GetUsersByUserNameOrFullName(string searchString, bool callerIsAdmin = false);
        //        IEnumerable<Gym> GetGyms(string cityName, string gymName, int maxCount);
        //        IEnumerable<Gym> SearchGyms(long lastGroupId, string searchText, double? leastRelevance, int limit);
        //        Group GetGroup(int groupId);
        //        void UserSetPrimaryGym(string userName, int gymId);
        //        void UserRemovePrimaryGym(string userName);
        //        void SetOrUpdateGroupMember(int groupId, string userName, UserGroupRole role);
        //        long? InsertPost(Post post);
        //        void UpdatePost(long postId, string description);
        //        void InsertReport(string authenticatedUserName, string reportedUser, long? reportedPost, long? reportedPostComment, string reason);
        //        User AdminGetUser(string userName);
        //        IEnumerable<Post> GetPostsFromOwnUser(string userName);
        //        IEnumerable<Post> GetPostsFromForeignUser(string userName, bool callerIsAdmin = false);
        //        IEnumerable<Post> GetAllPostsFromUser(Guid subId);
        //        IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        //        IEnumerable<Post> GetGroupPosts(int groupId, long startOffsetPostId, string searchText, double? leastRelevance, int limit, bool callerIsAdmin = false);
        //        IEnumerable<Post> GetNewsfeedPosts(string userName, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        //        Post GetPost(long postId);
        //        void DeletePostWithFlag(long postId);
        //        void InsertOrReplacePostLikeIfNewer(PostLike postLike);
        //        void InsertOrUpdatePostSubIfNewer(PostSub postSub);
        //        IEnumerable<PostLike> GetAllPostLikesFromUserSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<PostSub> GetAllPostSubsFromUserSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<User> GetPostSubbedBy(long postId);
        //        bool IsUserSubbedToPost(string userName, long postId);
        //        int InsertOrReplaceFollowIfNewer(Follow follow);
        //        IEnumerable<Follow> GetAllFollowsFromUserSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<Follow> GetAllFollowersFromUserSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<Follow> GetPendingFollowersFromUser(string userName);

        //        bool IsUser1FollowingUser2(string userName1, string userName2);
        //        void InsertUserDeviceEndpoint(string userName, string endpointArn, string deviceType, string deviceToken);
        //        void DeleteUserDeviceEndpoint(string userName, string deviceToken);
        //        IEnumerable<Device> GetUserDevices(string userName);
        //        void AdminSetUserDeactivatedStatus(string userName, bool isDeactivated);
        //        int UpdateFollowToAccepted(string follower, string following);
        //        void UpdateFollowToDenied(string follower, string following);
        //        long? InsertNotification(Notification notification);
        //        IEnumerable<PostComment> GetPostComments(long postId);
        //        long? InsertPostComment(PostComment postComment);

        //        PostComment GetPostComment(long postCommentId);
        //        void DeletePostCommentWithFlag(long postCommentId);
        //        IEnumerable<User> GetPostLikedBy(long postId);
        //        IEnumerable<User> GetUserFollowedByUser(string userName, string offsetOldestUserName, int limit);
        //        IEnumerable<User> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit);
        //        IEnumerable<User> GetGroupMembers(int groupId, string searchText, string offsetOldestUserName, int limit);
        //        void AdminSetPostDeactivatedStatus(long postId, bool isDeactivated);

        //        /*
        //* Creates a direct COnversation if not existing. Returns the existing or new created ConversationId for the 2 users.
        //*/
        //        long? CreateDirectConversationIfNotExist(string userName, string toUserName);

        //        IEnumerable<Conversation> GetNewOrUpdatedConversations(string userName, DateTime lastSyncTime);
        //        IEnumerable<Conversation_Participant> GetConversationParticipants(long conversationId);
        //        void UpdateConversationParticipantIfNewer(Conversation_Participant cP);
        //        int InsertOrIgnoreChatMessage(ChatMessage chatMessage);
        //        ChatMessage GetChatMessage(Guid messageId);
        //        IEnumerable<Conversation_Participant> GetNewOrUpdatedConversationParticipants(string userName, DateTime lastSync);
        //        void AdminSetReportHandled(long reportId, string userName, string actionTaken);

        //        //[Obsolete]
        //        //IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync, bool withAttachments);
        //        IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync);
        //        void InsertOrIgnoreAttachment(ChatMessage_Attachment attachment);
        //        ChatMessage_Attachment GetChatMessageAttachment(Guid attachmentId);
        //        int InsertOrIgnoreChatMessageWithAttachments(ChatMessage chatMessage);
        //        IEnumerable<Notification> GetNotifications(string userName, DateTime sinceDateTime);
        //        int GetGroupMemberCount(int groupId);
        //        void DeleteNotifications(string from, string to, NotificationType notificationType, long postId = -1);
        //        void InsertFeedback(string userName, string subject, string text);
        //        void InsertOrUpdateBlockedUserIfNewer(BlockedUser blockedUser);
        //        IEnumerable<BlockedUser> GetAllBlockedUsersFromUserSinceDate(string userName, DateTime sinceDate);
        //        bool IsUser1BlockedByUser2(string blockedUserName1, string userName2);
        //        IEnumerable<BlockedUser> GetBlockingUsersFor(string userName);
        //        void RemoveAllPostSubsFromUser1OnUser2(string blockedUserName1, string userName2);
        //        IEnumerable<Report> AdminGetReports(bool isHandled, long lastReportId, int limit);
        //        void DeleteUserWithFlag(string userName);

        //        void InsertOrUpdateWorkoutPlanIfNewer(WorkoutPlan wp);
        //        void InsertOrUpdateExerciseIfNewer(Exercise ex);
        //        void InsertOrUpdateWorkoutPlanExerciseIfNewer(string userName, WorkoutPlanExercise wpEx);
        //        IEnumerable<Exercise> GetAllExercisesSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<WorkoutPlan> GetAllWorkoutPlansSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<WorkoutPlanExercise> GetAllWorkoutPlanExercisesSinceDate(string userName, DateTime sinceDate);
        //        IEnumerable<Equipment> GetEquipmentSinceDate(DateTime sinceDate);
        //        IEnumerable<Muscle> GetMusclesSinceDate(DateTime sinceDate);
        //        void InsertOrUpdateWorkoutIfNewer(string userName, Workout w);
        //        IEnumerable<Workout> GetAllWorkoutsSinceDate(string userName, DateTime sinceDate);
        //        WorkoutPlanSyncData GetPublicWorkoutPlans(string userName);
        //        void CopyWorkoutPlan(Guid workoutPlanId, string userName);
        //        void InsertOrUpdateNotificationSetting(string userName, NotificationSetting setting);
        //        IEnumerable<NotificationSetting> GetNotificationSettings(string userName, DateTime modifiedSince = default(DateTime));

        Task<bool> GetUserHasCreatedProfile(string username);
        Task SetUserHasCreatedProfile(string username);
        Task EditUserProfile(User user);
        Task<User> GetUser(string username, bool IsOwnProfile);
        Task<User> AdminGetUserOnly(string username);
        Task<IEnumerable<User>> GetUsersForClearing();
        Task ClearUser(Guid subId);
        Task<IEnumerable<User>> GetUsersByUserNameOrFullName(string searchString, bool callerIsAdmin = false);
        Task<IEnumerable<Gym>> GetGyms(string cityName, string gymName, int maxCount);
        Task<IEnumerable<Gym>> SearchGyms(long lastGroupId, string searchText, double? leastRelevance, int limit);
        Task<Group> GetGroup(int groupId);
        Task UserSetPrimaryGym(string userName, int gymId);
        Task UserRemovePrimaryGym(string userName);
        Task SetOrUpdateGroupMember(int groupId, string userName, UserGroupRole role);
        Task<long?> InsertPost(Post post);
        Task UpdatePost(long postId, string description);
        Task InsertReport(string authenticatedUserName, string reportedUser, long? reportedPost, long? reportedPostComment, string reason);
        Task<User> AdminGetUser(string userName);
        Task<IEnumerable<Post>> GetPostsFromOwnUser(string userName);
        Task<IEnumerable<Post>> GetPostsFromForeignUser(string userName, bool callerIsAdmin = false);
        Task<IEnumerable<Post>> GetAllPostsFromUser(Guid subId);
        Task<IEnumerable<Post>> GetGroupPosts(int groupId, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        Task<IEnumerable<Post>> GetGroupPosts(int groupId, long startOffsetPostId, string searchText, double? leastRelevance, int limit, bool callerIsAdmin = false);
        Task<IEnumerable<Post>> GetNewsfeedPosts(string userName, long startOffsetPostId, int limit, bool callerIsAdmin = false);
        Task<Post> GetPost(long postId);
        Task DeletePostWithFlag(long postId);
        Task InsertOrReplacePostLikeIfNewer(PostLike postLike);
        Task InsertOrUpdatePostSubIfNewer(PostSub postSub);
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
        Task AdminSetUserDeactivatedStatus(string userName, bool isDeactivated);
        Task<int> UpdateFollowToAccepted(string follower, string following);
        Task UpdateFollowToDenied(string follower, string following);
        Task<long?> InsertNotification(Notification notification);
        Task<IEnumerable<PostComment>> GetPostComments(long postId);
        Task<long?> InsertPostComment(PostComment postComment);

        Task<PostComment> GetPostComment(long postCommentId);
        Task DeletePostCommentWithFlag(long postCommentId);
        Task<IEnumerable<User>> GetPostLikedBy(long postId);
        Task<IEnumerable<User>> GetUserFollowedByUser(string userName, string offsetOldestUserName, int limit);
        Task<IEnumerable<User>> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit);
        Task<IEnumerable<User>> GetGroupMembers(int groupId, string searchText, string offsetOldestUserName, int limit);
        Task AdminSetPostDeactivatedStatus(long postId, bool isDeactivated);

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
        Task AdminSetReportHandled(long reportId, string userName, string actionTaken);

        //[Obsolete]
        //IEnumerable<ChatMessage> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync, bool withAttachments);
        Task<IEnumerable<ChatMessage>> GetChatMessagesforUserSinceDate(string userName, DateTime lastSync);
        Task InsertOrIgnoreAttachment(ChatMessage_Attachment attachment);
        Task<ChatMessage_Attachment> GetChatMessageAttachment(Guid attachmentId);
        Task<int>InsertOrIgnoreChatMessageWithAttachments(ChatMessage chatMessage);
        Task<IEnumerable<Notification>> GetNotifications(string userName, DateTime sinceDateTime);
        Task<int>GetGroupMemberCount(int groupId);
        Task DeleteNotifications(string from, string to, NotificationType notificationType, long postId = -1, long? postCommentId = null);
        Task InsertFeedback(string userName, string subject, string text);
        Task InsertOrUpdateBlockedUserIfNewer(BlockedUser blockedUser);
        Task<IEnumerable<BlockedUser>> GetAllBlockedUsersFromUserSinceDate(string userName, DateTime sinceDate);
        Task<bool> IsUser1BlockedByUser2(string blockedUserName1, string userName2);
        Task<IEnumerable<BlockedUser>> GetBlockingUsersFor(string userName);
        Task RemoveAllPostSubsFromUser1OnUser2(string blockedUserName1, string userName2);
        Task<IEnumerable<Report>> AdminGetReports(bool isHandled, long lastReportId, int limit);
        Task DeleteUserWithFlag(string userName);

        Task InsertOrUpdateWorkoutPlanIfNewer(WorkoutPlan wp);
        Task InsertOrUpdateExerciseIfNewer(Exercise ex);
        Task InsertOrUpdateWorkoutPlanExerciseIfNewer(string userName, WorkoutPlanExercise wpEx);
        Task<IEnumerable<Exercise>> GetAllExercisesSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<WorkoutPlan>> GetAllWorkoutPlansSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<WorkoutPlanExercise>> GetAllWorkoutPlanExercisesSinceDate(string userName, DateTime sinceDate);
        Task<IEnumerable<Equipment>> GetEquipmentSinceDate(DateTime sinceDate);
        Task<IEnumerable<Muscle>> GetMusclesSinceDate(DateTime sinceDate);
        Task InsertOrUpdateWorkoutIfNewer(string userName, Workout w);
        Task<IEnumerable<Workout>> GetAllWorkoutsSinceDate(string userName, DateTime sinceDate);
        Task<WorkoutPlanSyncData> GetPublicWorkoutPlans(string userName);
        Task CopyWorkoutPlan(Guid workoutPlanId, string userName);
        Task InsertOrUpdateNotificationSetting(string userName, NotificationSetting setting);
        Task<IEnumerable<NotificationSetting>> GetNotificationSettings(string userName, DateTime modifiedSince = default(DateTime));
        Task<long?> GetPostCommentIdBy_UserName_Post_Time(string userName, long postId, DateTime? timePosted);
        Task<IEnumerable<Post>> GetExplorePosts();

        Task InsertOrReplacePostCommentLike(PostCommentLike postLike);
        Task<IEnumerable<PostCommentLike>> GetAllPostCommentLikesFromUserSinceDate(string userName, DateTime sinceDate);

        Task<IEnumerable<User>> GetPostCommentLikedBy(long postCommentId);
        Task<IEnumerable<Post>> GetSponsoredPosts(int brandId = -1, int limit = 9999);
        Task<IEnumerable<Brand>> GetBrands();
    }
}
