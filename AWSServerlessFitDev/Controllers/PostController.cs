using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PostController : Controller
    {
        IDatabaseService DbService { get; set; }
        IS3Service S3Client { get; set; }
        ILogger<PostController> Logger { get; set; }
        INotificationService NotifyService { get; set; }
        private readonly IFireForgetRepositoryHandler FireForgetRepositoryHandler;

        public PostController(Services.IDatabaseService dbService, IConfiguration configuration, IS3Service s3Client, 
            INotificationService iNotifyService, IFireForgetRepositoryHandler fireForgetRepositoryHandler,
            ILogger<PostController> logger)
        {
            DbService = dbService;
            NotifyService = iNotifyService;
            S3Client = s3Client;
            Logger = logger;
            FireForgetRepositoryHandler = fireForgetRepositoryHandler;
        }



        [HttpPost]
        public async Task<IActionResult> PostPost()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            Post post = null;
            try
            {
                post = await ApiPayloadClass<Post>.GetRequestValueAsync(S3Client, Request.Body);
                if (post == null)
                {
                    //await S3Client.Delete(requestFilePath);
                    Logger.LogWarning("Could not Post Post. Post is null. UserName={userName}", authenticatedUserName);
                    return BadRequest();
                }
                else if (post.PostResource != null && post.PostResource.Length > 100000000)
                {
                    //await S3Client.Delete(requestFilePath);
                    Logger.LogWarning("Could not Post Post. Post to big. Length={length} UserName={userName}", post.PostResource.Length, authenticatedUserName);
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status413PayloadTooLarge);
                }
                else if (post.UserName.ToLower() != authenticatedUserName.ToLower())
                    return Unauthorized();
                else if(post.IsProfilePost && post.GroupId != null)
                {
                    return BadRequest();
                }
                else if(!post.IsProfilePost && post.GroupId == null)
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            string uniqueFileName;
            string uniqueFileNameThumbnail;

            if (post.PostType == PostType.Image)
            {
                if (!Utils.IsValidImage(post.PostResource))
                {
                    //await S3Client.Delete(requestFilePath);
                    Logger.LogWarning("Could not Post Post. Post Resource no valid image. UserName={userName}", authenticatedUserName);
                    return BadRequest();
                }
                if (post.IsProfilePost)
                {
                    uniqueFileName = string.Format(@"{0}_{1}.jpg", post.UserName, Guid.NewGuid());
                    uniqueFileNameThumbnail = string.Format(@"{0}_{1}.jpg", post.UserName, Guid.NewGuid());
                }
                else
                {
                    uniqueFileName = string.Format(@"{0}_{1}_{2}.jpg", post.GroupId, post.UserName, Guid.NewGuid());
                    uniqueFileNameThumbnail = string.Format(@"{0}_{1}_{2}.jpg", post.GroupId, post.UserName, Guid.NewGuid());
                }


                using (MemoryStream stream = new MemoryStream(post.PostResource))
                {
                    using (MemoryStream jpegStream = new MemoryStream())
                    {
                        Image.Load(stream).SaveAsJpeg(jpegStream);
                        post.PostResourceUrl = await S3Client.PutObjectAsync(S3Client.GymnectS3DataFolder, uniqueFileName, jpegStream);
                    }

                    stream.Position = 0;
                    //Create Compressed Image
                    using (MemoryStream lowResJpegStream = new MemoryStream())
                    using (Image lowResImage = Image.Load(stream))
                    {
                        lowResImage.Mutate(x => x.Resize(200, 0));
                        lowResImage.SaveAsJpeg(lowResJpegStream, new JpegEncoder() { Quality = 80 });
                        post.PostResourceThumbnailUrl = await S3Client.PutObjectAsync(S3Client.GymnectS3DataFolder, uniqueFileNameThumbnail, lowResJpegStream);
                    }
                }

            }
            else if (post.PostType == PostType.Video)
            {
                if (post.PostResourceThumbnail == null || post.PostResourceThumbnail.Length < 1)
                {
                    return BadRequest();
                }
                if (post.IsProfilePost)
                {
                    uniqueFileName = string.Format(@"{0}_{1}.mp4", post.UserName, Guid.NewGuid());
                    uniqueFileNameThumbnail = string.Format(@"{0}_{1}.jpg", post.UserName, Guid.NewGuid());
                }
                else
                {
                    uniqueFileName = string.Format(@"{0}_{1}_{2}.mp4", post.GroupId, post.UserName, Guid.NewGuid());
                    uniqueFileNameThumbnail = string.Format(@"{0}_{1}_{2}.jpg", post.GroupId, post.UserName, Guid.NewGuid());
                }


                using (MemoryStream videoStream = new MemoryStream(post.PostResource))
                {
                    post.PostResourceUrl = await S3Client.PutObjectAsync(S3Client.GymnectS3DataFolder, uniqueFileName, videoStream);
                }
                using (MemoryStream thumbnailStream = new MemoryStream(post.PostResourceThumbnail))
                {
                    post.PostResourceThumbnailUrl = await S3Client.PutObjectAsync(S3Client.GymnectS3DataFolder, uniqueFileNameThumbnail, thumbnailStream);
                }
            }
            else if (post.PostType == PostType.Text)
            {
                if (post.IsProfilePost || String.IsNullOrWhiteSpace(post.Text))
                    return BadRequest();
                post.PostResourceUrl = "";
                post.PostResourceThumbnailUrl = "";
            }
            long postId;
            try
            {
                post.CreatedAt = DateTime.UtcNow;
                post.LastModified = DateTime.UtcNow;
                postId = (long)await DbService.InsertPost(post);

                await DbService.InsertOrUpdatePostSubIfNewer(new PostSub()
                { UserName = post.UserName, PostId = postId, IsDeleted = false, LastModified = DateTime.UtcNow });
                //await S3Client.Delete(requestFilePath);
            }
            catch (Exception ex)
            {
                //await S3Client.Delete(requestFilePath);
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            //Notify TaggedUsers
            string textForNotifications = post.PostType == PostType.Text ? post.Text : post.Description;
            await NotifyTaggedUsers(textForNotifications, post.UserName, postId, NotificationType.PostLinking);

            //Notify all group members
            if(!post.IsProfilePost && post.GroupId != null)
            {
                FireForgetRepositoryHandler.Execute(async (dbService, notifyService) =>
                {
                    try
                    {
                        var groupMembers = (await DbService.GetGroupMembers(post.GroupId.Value, null, null, 5000)).ToList();
                        foreach (var member in groupMembers)
                        {
                            if (member.UserName.ToLower() == authenticatedUserName)
                                continue;
                            await notifyService.SendAlertNotification(member.UserName, authenticatedUserName + " " + Constants.Strings.PostInGymPublishedString, NotificationType.GymPostPublished);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex.ToString());
                    }
                });
            }

            //return Ok(new { Value = postId });
            return Ok(ApiPayloadClass<long>.CreateSmallApiResponse(postId));
        }

        private async Task NotifyTaggedUsers(string text, string fromUserName, long postId, NotificationType notificationType, long? commentId = null)
        {
            try
            {
                //Notify TaggedUsers
                var taggedUsers = StringHelper.GetTaggedUsers(text);
                foreach (string userName in taggedUsers)
                {
                    try
                    {
                        if (!await DbService.IsUser1BlockedByUser2(fromUserName, userName) && !await DbService.IsUser1BlockedByUser2(userName, fromUserName))
                            await NotifyService.SendNotification(fromUserName, userName, notificationType, content: commentId, postId: postId, saveToDatabase: true, publish: true);
                    }
                    catch (Exception sendNotificationException)
                    {
                        Logger.LogException("", sendNotificationException, Request);
                    }
                }
            }
            catch (Exception ex2)
            {
                Logger.LogException("", ex2, Request);
            }
        }

        [HttpPut]
        [Route("{postId:long}")]
        public async Task<IActionResult> UpdatePost([FromRoute] long postId, [FromQuery] string description)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            Post post = null;
            try
            {
                post = await DbService.GetPost(postId);

                if (post == null)
                    return BadRequest();
                else if (post.UserName.ToLower() != authenticatedUserName.ToLower())
                    return Unauthorized();
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            try
            {
                if (post.PostType == PostType.Text)
                {
                    if (String.IsNullOrWhiteSpace(description))
                        return BadRequest();
                }


                if (String.IsNullOrWhiteSpace(description))
                    description = "";
                await DbService.UpdatePost(postId, description);

                //Notify TaggedUsers
                string textForNotifications = description;
                await NotifyTaggedUsers(textForNotifications, post.UserName, postId, NotificationType.PostLinking);

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }
        }


        [Route("{postId:long}")]
        [HttpGet]
        public async Task<IActionResult> GetPost(long postId)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            Post post = await DbService.GetPost(postId);

            if (post == null || post.IsDeleted == true || post.IsDeactivated || await DbService.IsUser1BlockedByUser2(authenticatedUserName, post.UserName))
            {
                post = null;
                //return Ok(new { Value = post });
                return Ok(ApiPayloadClass<Post>.CreateSmallApiResponse(post));
            }

            if (post.IsProfilePost)
            {
                User postOwner = await DbService.GetUser(post.UserName, false);
                if (postOwner == null)
                    return BadRequest();
                if (postOwner.IsPrivate)
                {
                    if (!postOwner.UserName.ToLower().Equals(authenticatedUserName.ToLower()))
                    {
                        if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, postOwner.UserName))
                        {
                            return Unauthorized();
                        }
                    }

                }
            }

            post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, 60 * 10);
            post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, 60 * 10);

            //return Ok(new { Value = post });
            return Ok(ApiPayloadClass<Post>.CreateSmallApiResponse(post));

        }


        [Route("User/{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetPostsFromForeignUser(string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            if (await DbService.IsUser1BlockedByUser2(authenticatedUserName, userName))
                return NotFound();

            User postOwner = await DbService.GetUser(userName, false);
            if (postOwner == null)
                return NotFound();

            if (postOwner.IsPrivate)
            {
                if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, postOwner.UserName))
                {
                    return Unauthorized();
                }
            }

            List<Post> userPosts = (await DbService.GetPostsFromForeignUser(userName))?.ToList();

            if (userPosts != null)
            {
                foreach (Post post in userPosts)
                {
                    post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (60 * 24));
                    post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (60 * 24));
                }
            }

            //return Ok(new { Value = userPosts });
            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(userPosts));

        }


        [Route("SyncFromServer")]
        [HttpPost]
        public async Task<IActionResult> SyncProfilePostsFromServer()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<Post> clientPosts = null;
            try
            {
                clientPosts = await ApiPayloadClass<List<Post>>.GetRequestValueAsync(S3Client, Request.Body);
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            List<Post> serverPosts = (await DbService.GetPostsFromOwnUser(authenticatedUserName)).ToList();

            if (serverPosts != null)
            {
                foreach (Post post in serverPosts)
                {
                    post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, 60);
                    post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, 60);
                }
            }


            if (clientPosts == null || clientPosts.Count < 1)
            {
                return Ok(new { Value = serverPosts });
            }
            else
            {//holle alle posts, dessen id nicht in clientpost enthalten ist + Lastmodified neuer
                List<Post> newOrUpdatedProfilePosts = serverPosts.Where(p => !clientPosts.Any(p2 => p2.PostId == p.PostId)
                || clientPosts.Where(p3 => p3.PostId == p.PostId).FirstOrDefault().LastModified.TrimMilliseconds() < p.LastModified.TrimMilliseconds()
                ).ToList();

                //return Ok(new { Value = newOrUpdatedProfilePosts });
                return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(newOrUpdatedProfilePosts));

            }
        }

        [Route("{postId:long}")]
        [HttpDelete]
        public async Task<IActionResult> DeletePost(long postId)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            Post post = await DbService.GetPost(postId);

            if (post == null)
                return BadRequest();
            else if (post.UserName.ToLower() != authenticatedUserName.ToLower())
                return Unauthorized();

            try
            {
                await DbService.DeletePostWithFlag(postId);
                Logger.LogInformation("PostId={postId} was deleted by UserName={userName}", postId, authenticatedUserName);
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }
        }


        [Route("Like/BiSync")]
        [HttpPost]
        public async Task<IActionResult> PostLikeBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<PostLike> clientPostLikes = null;
            DateTime? lastSync = null;
            SyncRequest<List<PostLike>> postLikeSyncReq = null;
            try
            {
                postLikeSyncReq = await ApiPayloadClass<SyncRequest<List<PostLike>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientPostLikes = postLikeSyncReq?.ObjectChangedOnClient;
                lastSync = postLikeSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            if (clientPostLikes != null)
            {
                List<BlockedUser> usersThatBlockedCaller = (await DbService.GetBlockingUsersFor(authenticatedUserName)).ToList();
                foreach (PostLike clientPostLike in clientPostLikes)
                {
                    try
                    {
                        if (clientPostLike.UserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        Post post = await DbService.GetPost(clientPostLike.PostId);


                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == post.UserName))
                            continue;

                        if (post.IsProfilePost)
                        {
                            User postOwningUser = await DbService.GetUser(post.UserName, false);
                            if (postOwningUser == null)
                                continue;
                            if (postOwningUser.IsPrivate)
                            {
                                if (!postOwningUser.UserName.Equals(clientPostLike.UserName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (!await DbService.IsUser1FollowingUser2(clientPostLike.UserName, postOwningUser.UserName))
                                    {
                                        continue;
                                    }
                                }
                            }
                        }


                        await DbService.InsertOrReplacePostLikeIfNewer(clientPostLike);

                        if (clientPostLike.UserName.ToLower() != post.UserName.ToLower())
                        {
                            if (clientPostLike.IsDeleted == false)
                            {
                                bool shouldPublish = await DbService.IsUserSubbedToPost(post.UserName, clientPostLike.PostId);
                                await NotifyService.SendNotification(clientPostLike.UserName, post.UserName, NotificationType.PostLike, postId: clientPostLike.PostId, saveToDatabase: true, publish: shouldPublish);
                            }
                            else if (clientPostLike.IsDeleted == true)
                            {
                                await DbService.DeleteNotifications(clientPostLike.UserName, post.UserName, NotificationType.PostLike, postId: clientPostLike.PostId);
                                await NotifyService.SendNotification(clientPostLike.UserName, post.UserName, NotificationType.PostUnlike, postId: clientPostLike.PostId, saveToDatabase: false);
                            }
                        }
                    }
                    catch (Exception ex2) 
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }

                }
            }

            //Get PostLikes with LastModified > lastSync
            List<PostLike> serverPostLikes = new List<PostLike>();
            DateTime sinceDateTime = lastSync ?? DateTime.MinValue;

            serverPostLikes = (await DbService.GetAllPostLikesFromUserSinceDate(authenticatedUserName, sinceDateTime)).ToList();

            return Ok(await ApiPayloadClass<List<PostLike>>.CreateApiResponseAsync(S3Client, serverPostLikes));
        }


        [Route("{postId:long}/Comments")]
        [HttpGet]
        public async Task<IActionResult> GetPostComments(long postId)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            Post post = await DbService.GetPost(postId);

            if (post == null || post.IsDeleted == true || post.IsDeactivated)
            {
                post = null;
                return Ok(new { Value = post });
            }

            bool isOwnProfilePost = false;

            if (post.IsProfilePost)
            {
                User postOwner = await DbService.GetUser(post.UserName, false);
                if (postOwner == null)
                    return BadRequest();

                if (postOwner.UserName.ToLower().Equals(authenticatedUserName.ToLower()))
                    isOwnProfilePost = true;

                if (postOwner.IsPrivate)
                {
                    if (!isOwnProfilePost)
                    {
                        if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, postOwner.UserName))
                        {
                            return Unauthorized();
                        }
                    }
                }
            }

            List<BlockedUser> usersThatBlockedCaller = (await DbService.GetBlockingUsersFor(authenticatedUserName)).ToList();
            List<BlockedUser> blockedUsers = (await DbService.GetAllBlockedUsersFromUserSinceDate(authenticatedUserName, DateTime.MinValue)).Where(x => x.IsDeleted == false).ToList();

            List<PostComment> allPostComments = (await DbService.GetPostComments(postId))?.ToList();
            List<PostComment> resultPostComments = new List<PostComment>();

            if (isOwnProfilePost)
            {
                resultPostComments = allPostComments;
            }
            else
            {
                foreach (PostComment pc in allPostComments)
                {
                    if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == pc.UserName.ToLower()) || blockedUsers.Any(bu => bu.BlockedUserName.ToLower() == pc.UserName.ToLower()))
                        continue;
                    resultPostComments.Add(pc);
                }
            }


            return Ok(await ApiPayloadClass<List<PostComment>>.CreateApiResponseAsync(S3Client, resultPostComments));
        }


        [HttpPost("Comments")]
        public async Task<IActionResult> PostPostComments()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<PostComment> postComments = null;
            try
            {
                postComments = await ApiPayloadClass<List<PostComment>>.GetRequestValueAsync(S3Client, Request.Body);
                if (postComments == null)
                {
                    Logger.LogWarning("Could not Post PostComment. postComments is null. UserName={userName}", authenticatedUserName);
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            try
            {
                List<BlockedUser> usersThatBlockedCaller = (await DbService.GetBlockingUsersFor(authenticatedUserName)).ToList();
                foreach (var comment in postComments)
                {
                    try
                    {
                        if (comment.UserName.ToLower() != authenticatedUserName.ToLower())
                            return BadRequest();

                        //check if comment was already posted
                        long? postedCommentId = await DbService.GetPostCommentIdBy_UserName_Post_Time(comment.UserName, comment.PostId, comment.TimePosted);
                        if(postedCommentId != null && postedCommentId >= 0)
                        {
                            comment.ServerId = postedCommentId;
                            continue;
                        }

                        Post post = await DbService.GetPost(comment.PostId);

                        if (post.IsDeleted || post.IsDeactivated)
                            continue;

                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == post.UserName.ToLower()))
                            continue;

                        User postOwningUser = await DbService.GetUser(post.UserName, false);
                        if (postOwningUser == null)
                            continue;
                        if (comment.PostId < 0)
                            continue;
                        if (postOwningUser.UserName.ToLower() != comment.UserName.ToLower()
                            && post.IsProfilePost && postOwningUser.IsPrivate
                            && !await DbService.IsUser1FollowingUser2(comment.UserName, postOwningUser.UserName))
                            continue;

                        long? postCommentId = await DbService.InsertPostComment(comment);
                        comment.ServerId = postCommentId;

                        //Notify TaggedUsers
                        string textForNotifications = comment.Text;
                        await NotifyTaggedUsers(textForNotifications, comment.UserName, comment.PostId, NotificationType.CommentLinking, comment.ServerId);


                        //Notify subbed Users of Post
                        List<User> subbedUsers = (await DbService.GetPostSubbedBy(comment.PostId)).ToList();

                        //Post owning User has unsubscribed from Post Notifications, but a Notification has to be inserted in the database without sending it
                        if (!subbedUsers.Any(u => u.UserName.ToLower() == postOwningUser.UserName.ToLower()))
                        {
                            await NotifyService.SendNotification(comment.UserName, postOwningUser.UserName, NotificationType.PostComment, postId: comment.PostId, publish: false);
                        }

                        foreach (var sU in subbedUsers)
                        {
                            if (sU.UserName.ToLower() != comment.UserName.ToLower())
                            {
                                //Postowner receives notification
                                if (sU.UserName.ToLower() == postOwningUser.UserName.ToLower())
                                    await NotifyService.SendNotification(comment.UserName, sU.UserName, NotificationType.PostComment, postId: comment.PostId);
                                else//Subbed User Receives Notification
                                    await NotifyService.SendNotification(comment.UserName, sU.UserName, NotificationType.SubbedPostComment, postId: comment.PostId);
                            }
                        }

                    }
                    catch (Exception ex1)
                    {
                        Logger.LogException(authenticatedUserName, ex1, Request);
                    }
                }
                return Ok(ApiPayloadClass<List<PostComment>>.CreateSmallApiResponse(postComments));
            }
            catch (Exception ex2)
            {
                return BadRequest();
            }
        }


        [Route("Comment/{postCommentId:long}")]
        [HttpDelete]
        public async Task<IActionResult> DeletePostComment(long postCommentId)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            PostComment postComment = await DbService.GetPostComment(postCommentId);

            if (postComment == null)
            {
                return BadRequest();
            }

            try
            {
                if (authenticatedUserName.ToLower() == postComment.UserName.ToLower())
                {
                    await DbService.DeletePostCommentWithFlag(postCommentId);
                    Logger.LogInformation("PostCommentId={postCommentId} was deleted by UserName={userName}", postCommentId, authenticatedUserName);
                    return Ok();
                }
                else
                {
                    Post post = await DbService.GetPost(postComment.PostId);
                    if (post.UserName.ToLower() == authenticatedUserName.ToLower() && post.IsProfilePost == true)
                    {
                        await DbService.DeletePostCommentWithFlag(postCommentId);
                        Logger.LogInformation("PostCommentId={postCommentId} was deleted by UserName={userName}", postCommentId, authenticatedUserName);
                        return Ok();
                    }
                    else
                        return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }
        }

        [Route("{postId:long}/Likes")]
        [HttpGet]
        public async Task<IActionResult> GetPostLikedBy(long postId)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            Post post = await DbService.GetPost(postId);

            if (post == null || post.IsDeleted == true || post.IsDeactivated)
            {
                post = null;
                return Ok(new { Value = post });
            }
            if (post.IsProfilePost)
            {
                User postOwner = await DbService.GetUser(post.UserName, false);
                if (postOwner == null)
                    return BadRequest();
                if (postOwner.IsPrivate)
                {
                    if (!postOwner.UserName.ToLower().Equals(authenticatedUserName.ToLower()))
                    {
                        if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, postOwner.UserName))
                        {
                            return Unauthorized();
                        }
                    }
                }
            }
            List<User> users = (await DbService.GetPostLikedBy(postId))?.ToList();
            //return Ok(new { Value = users });
            return Ok(await ApiPayloadClass<List<User>>.CreateApiResponseAsync(S3Client, users));
        }



        //[Route("Group/{groupId:int}/{startOffsetPostId:long}/{limit:int}")]
        //[HttpGet]
        //public async Task<IActionResult> GetPostsFromGroup(int groupId, long startOffsetPostId, int limit)
        [Route("Group/{groupId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetPostsFromGroup([FromRoute] int groupId, [FromQuery] long startOffsetPostId, [FromQuery] string searchText, [FromQuery] double? leastRelevance, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            if (groupId < 0 || limit < 0 || limit > 50)
                return BadRequest();
            if (String.IsNullOrWhiteSpace(searchText))
                searchText = null;

            Group group = await DbService.GetGroup(groupId);
            if (group == null)
                return BadRequest();
            List<Post> resultPostList = new List<Post>();
            if (group.PrivacyType != GroupPrivacyTypes.Public)
            {
                //Todo
            }
            else
            {
                List<Post> posts = (await DbService.GetGroupPosts(groupId, startOffsetPostId, searchText, leastRelevance, limit))?.ToList();
                List<BlockedUser> usersThatBlockedCaller = (await DbService.GetBlockingUsersFor(authenticatedUserName)).ToList();
                List<BlockedUser> blockedUsers = (await DbService.GetAllBlockedUsersFromUserSinceDate(authenticatedUserName, DateTime.MinValue)).Where(x => x.IsDeleted == false).ToList();

                int loopCount = 0;
                //If alle next 10 posts are from a blocked user, we have to get the next 10 posts
                while (resultPostList.Count == 0 && posts.Count > 0 && loopCount < 10)
                {
                    loopCount++;
                    posts = (await DbService.GetGroupPosts(groupId, startOffsetPostId, searchText, leastRelevance, limit))?.ToList();

                    foreach (Post post in posts)
                    {
                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == post.UserName.ToLower()) || blockedUsers.Any(bu => bu.BlockedUserName.ToLower() == post.UserName.ToLower()))
                            continue;
                        post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (7 * 60 * 24));
                        post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
                        resultPostList.Add(post);
                    }

                    if (resultPostList.Count == 0 && posts.Count > 0)
                        startOffsetPostId = (long)posts.Min(i => i.PostId);
                }
            }
            //return Ok(new { Value = posts });
            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(resultPostList));
        }

        [Route("Newsfeed")]
        [HttpGet]
        public async Task<IActionResult> GetNewsfeedPosts([FromQuery] long startOffsetPostId, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<Post> posts = new List<Post>();

            posts = (await DbService.GetNewsfeedPosts(authenticatedUserName, startOffsetPostId, limit))?.ToList();
            foreach (Post post in posts)
            {
                post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (60 * 24 * 7));
                post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (60 * 24 * 7));
            }

            //return Ok(new { Value = posts });
            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(posts));
        }



        [Route("Subscription/BiSync")]
        [HttpPost]
        public async Task<IActionResult> PostSubBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<PostSub> clientPostSubs = null;
            DateTime? lastSync = null;
            SyncRequest<List<PostSub>> postSubSyncReq = null;
            try
            {
                postSubSyncReq = await ApiPayloadClass<SyncRequest<List<PostSub>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientPostSubs = postSubSyncReq?.ObjectChangedOnClient;
                lastSync = postSubSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            if (clientPostSubs != null)
            {
                List<BlockedUser> usersThatBlockedCaller = (await DbService.GetBlockingUsersFor(authenticatedUserName)).ToList();
                foreach (PostSub clientPostSub in clientPostSubs)
                {
                    try
                    {
                        if (clientPostSub.UserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        Post post = await DbService.GetPost(clientPostSub.PostId);

                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == post.UserName))
                            continue;

                        if (post.IsProfilePost)
                        {
                            User postOwningUser = await DbService.GetUser(post.UserName, false);
                            if (postOwningUser == null)
                                continue;
                            if (postOwningUser.IsPrivate)
                            {
                                if (!await DbService.IsUser1FollowingUser2(clientPostSub.UserName, postOwningUser.UserName))
                                {
                                    continue;
                                }
                            }
                        }

                        await DbService.InsertOrUpdatePostSubIfNewer(clientPostSub);
                    }
                    catch (Exception ex2) 
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }

                }
            }

            //Get PostSub with LastModified > lastSync
            List<PostSub> serverPostSubs = new List<PostSub>();
            if (lastSync == null)
            {
                serverPostSubs = (await DbService.GetAllPostSubsFromUserSinceDate(authenticatedUserName, DateTime.MinValue)).ToList();
            }
            else
            {
                serverPostSubs = (await DbService.GetAllPostSubsFromUserSinceDate(authenticatedUserName, (DateTime)lastSync)).ToList();
            }
            return Ok(await ApiPayloadClass<List<PostSub>>.CreateApiResponseAsync(S3Client, serverPostSubs));
        }

        [Route("Explore")]
        [HttpGet]
        public async Task<IActionResult> GetExplorePosts()
        {
            List<Post> posts = new List<Post>();

            posts = (await DbService.GetExplorePosts())?.ToList();
            foreach (Post post in posts)
            {
                post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (60 * 24 * 7));
                post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (60 * 24 * 7));
            }

            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(posts));
        }


    }
}
