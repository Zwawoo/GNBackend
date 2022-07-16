using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        IDatabaseService DbService { get; set; }
        IS3Service S3Client { get; set; }
        ILogger<AdminController> Logger {get; set;}
        IEmailService EmailService { get; set; }
        INotificationService NotifyService { get; set; }
        public AdminController(Services.IDatabaseService dbService, IConfiguration configuration, 
            IS3Service s3Client, INotificationService iNotifyService, ILogger<AdminController> logger,
            IEmailService emailService)
        {
            DbService = dbService;
            NotifyService = iNotifyService;
            S3Client = s3Client;
            Logger = logger;
            EmailService = emailService;
        }


        [Route("User/{userName}")]
        [HttpGet]
        public async Task<IActionResult> AdminGetUser([FromRoute] string userName)
        {
            User user = null;

            user = DbService.AdminGetUser(userName);
            if (user != null)
            {
                user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 5);
                user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 5);
            }

            return Ok(ApiPayloadClass<User>.CreateSmallApiResponse(user));
        }

        [Route("Post/{postId:long}")]
        [HttpGet]
        public async Task<IActionResult> AdminGetPost([FromRoute] long postId)
        {
            Post post = DbService.GetPost(postId);

            if (post == null)
                return BadRequest();

            post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, 60 * 10);
            post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, 60 * 10);

            //return Ok(new { Value = post });
            return Ok(ApiPayloadClass<Post>.CreateSmallApiResponse(post));
        }

        [Route("Post/{postId:long}/Comments")]
        [HttpGet]
        public async Task<IActionResult> AdminGetPostComments([FromRoute] long postId)
        {
            List<PostComment> allPostComments = DbService.GetPostComments(postId)?.ToList();

            return Ok(await ApiPayloadClass<List<PostComment>>.CreateApiResponseAsync(S3Client, allPostComments));
        }

        [Route("User/{userName}/Disable")]
        [HttpPut]
        public async Task<IActionResult> AdminDisableUser([FromRoute] string userName)
        {
            try
            {
                await CognitoService.DisableUser(userName);

                Logger.LogInformation("UserName={username} was disabled by UserName={admin}", userName, Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString());

                DbService.AdminSetUserDeactivatedStatus(userName, true);

                User user = DbService.AdminGetUserOnly(userName);

                try
                {
                    string emailBody = $"Hallo {user.UserName}, <br><br>dein Profil wurde aufgrund eines Verstoßes gegen unsere Nutzungsbedingungen deaktiviert.<br> " +
                    $"Bitte wende dich bei Fragen an unseren Support (support@gymnect.de).<br><br>Dein Gymnect Team";
                    EmailService.SendEmail(user.Email, "Gymnect Benutzerdeaktivierung", emailBody);
                }
                catch(Exception ex0)
                {
                    Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex0, Request);
                }
                

                var userDevices = DbService.GetUserDevices(userName);
                foreach (var device in userDevices)
                {
                    DbService.DeleteUserDeviceEndpoint(userName, device.DeviceToken);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex, Request);
                return BadRequest(new { message = ex.ToString() });
            }
            return Ok();
        }

        [Route("User/{userName}/Enable")]
        [HttpPut]
        public async Task<IActionResult> AdminEnableUser([FromRoute] string userName)
        {
            await CognitoService.EnableUser(userName);

            Logger.LogInformation("UserName={username} was enabled by UserName={admin}", userName, Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString());

            User user = DbService.AdminGetUserOnly(userName);

            DbService.AdminSetUserDeactivatedStatus(userName, false);

            try
            {
                string emailBody = $"Hallo {user.UserName}, <br><br>dein Profil wurde wieder aktiviert.<br> " +
                        $"Damit du dein Profil wieder nutzen kannst, melde dich bitte bei Gymnect ab und erneut an.<br>" +
                    $"Bitte wende dich bei Fragen an unseren Support (support@gymnect.de).<br><br>Dein Gymnect Team";
                EmailService.SendEmail(user.Email, "Gymnect Benutzerreaktivierung", emailBody);
            }
            catch (Exception ex0)
            {
                Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex0, Request);
            }
            

            
            return Ok();
        }


        [Route("Post/{postId:long}/Deactivate")]
        [HttpPut]
        public async Task<IActionResult> AdminDeactivatePost([FromRoute] long postId)
        {
            DbService.AdminSetPostDeactivatedStatus(postId, true);

            Logger.LogInformation("PostId={postId} was deactivated by UserName={admin}", postId.ToString(), Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString());

            Post post = DbService.GetPost(postId);
            if(post != null)
            {
                User user = DbService.AdminGetUserOnly(post.UserName);
                if(user != null)
                {
                    try
                    {
                        string emailBody = $"Hallo {user.UserName}, <br><br>ein Beitrag von dir vom {((DateTime)post.CreatedAt).ToString("g", CultureInfo.GetCultureInfo("de-DE"))} wurde aufgrund eines Verstoßes gegen unsere Nutzungsbedingungen deaktiviert.<br> " +
                        $"Bitte wende dich bei Fragen an unseren Support (support@gymnect.de).<br><br>Dein Gymnect Team";
                        EmailService.SendEmail(user.Email, "Gymnect Beitragdeaktivierung", emailBody);
                    }
                    catch (Exception ex0)
                    {
                        Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex0, Request);
                    }
                    
                }         
            }
            
            return Ok();
        }

        [Route("Post/{postId:long}/Activate")]
        [HttpPut]
        public async Task<IActionResult> AdminActivatePost([FromRoute] long postId)
        {
            DbService.AdminSetPostDeactivatedStatus(postId, false);
            Logger.LogInformation("PostId={postId} was activated by UserName={admin}", postId.ToString(), Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString());
            return Ok();
        }


        [Route("Post/Comment/{postCommentId:long}")]
        [HttpDelete]
        public async Task<IActionResult> AdminDeletePostComment(long postCommentId)
        {
            DbService.DeletePostCommentWithFlag(postCommentId);
            Logger.LogInformation("postCommentId={postCommentId} was deactivated by UserName={admin}", postCommentId.ToString(), Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString());
            return Ok();
        }

        [Route("Post/Group/{groupId:int}")]
        [HttpGet]
        public async Task<IActionResult> AdminGetPostsFromGroup([FromRoute] int groupId, [FromQuery] long startOffsetPostId, [FromQuery] string searchText, [FromQuery] double? leastRelevance, [FromQuery] int limit)
        {
            if (groupId < 0 || limit < 0 || limit > 50)
                return BadRequest();
            if (String.IsNullOrWhiteSpace(searchText))
                searchText = null;

            Group group = DbService.GetGroup(groupId);
            if (group == null)
                return BadRequest();

            List<Post> posts = DbService.GetGroupPosts(groupId, startOffsetPostId, searchText, leastRelevance, limit, callerIsAdmin: true)?.ToList();

            foreach (Post post in posts)
            {
                post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (7 * 60 * 24));
                post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
            }
            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(posts));
        }

        [Route("Reports")]
        [HttpGet]
        public async Task<IActionResult> AdminGetReports([FromQuery] bool isHandled, [FromQuery] long lastReportId, [FromQuery] int limit)
        {
            List<Report> reports = DbService.AdminGetReports(isHandled, lastReportId, limit).ToList();
            return Ok(ApiPayloadClass<List<Report>>.CreateSmallApiResponse(reports));
        }

        [Route("Report/{reportId:long}")]
        [HttpPut]
        public async Task<IActionResult> AdminSetReportHandled([FromRoute] long reportId, [FromQuery] string actionTaken)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            DbService.AdminSetReportHandled(reportId, authenticatedUserName, actionTaken);
            return Ok();
        }

        [Route("Post/User/{userName}")]
        [HttpGet]
        public async Task<IActionResult> AdminGetPostsFromForeignUser([FromRoute]string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            User postOwner = DbService.AdminGetUser(userName);
            if (postOwner == null)
                return NotFound();

            List<Post> userPosts = DbService.GetPostsFromForeignUser(userName, callerIsAdmin: true)?.ToList();

            if (userPosts != null)
            {
                foreach (Post post in userPosts)
                {
                    post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (60 * 24));
                    post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (60 * 24));
                }
            }
            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(userPosts));
        }

        [Route("User/Search")]
        [HttpGet]
        public async Task<IActionResult> AdminGetUsers([FromQuery] string searchString)
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                if (String.IsNullOrEmpty(searchString) || searchString.Length > 128)
                    return BadRequest();

                List<User> users = DbService.GetUsersByUserNameOrFullName(searchString, true).ToList();

                foreach (User user in users)
                {
                    user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 5);
                    user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 5);
                }

                return Ok(ApiPayloadClass<List<User>>.CreateSmallApiResponse(users));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [Route("Post/Newsfeed")]
        [HttpGet]
        public async Task<IActionResult> AdminGetNewsfeedPosts([FromQuery] long startOffsetPostId, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<Post> posts = new List<Post>();

            posts = DbService.GetNewsfeedPosts(authenticatedUserName, startOffsetPostId, limit, callerIsAdmin: true)?.ToList();
            foreach (Post post in posts)
            {
                post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, (60 * 24 * 7));
                post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, (60 * 24 * 7));
            }

            return Ok(ApiPayloadClass<List<Post>>.CreateSmallApiResponse(posts));
        }


    }
}
