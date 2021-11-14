using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        IDatabaseService DbService { get; set; }
        S3Service S3Client { get; set; }
        INotificationService NotifyService { get; set; }
        private readonly IFireForgetRepositoryHandler FireForgetRepositoryHandler;

        public UserController(Services.IDatabaseService dbService, IConfiguration configuration, IAmazonS3 s3Client, 
            INotificationService iNotifyService, IFireForgetRepositoryHandler fireForgetRepositoryHandler)
        {
            DbService = dbService;
            S3Client = new S3Service(configuration, s3Client);
            NotifyService = iNotifyService;
            FireForgetRepositoryHandler = fireForgetRepositoryHandler;
        }


        [Route("{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetUser(string userName)
        {
            bool requestUserNameIsCaller = false;
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            if (authenticatedUserName.ToLower() == userName.ToLower())
                requestUserNameIsCaller = true;
            //try
            //{
            //    AuthenticationResult authRes = await Utils.CheckAuthentication(Request);
            //    if (authRes.IsAuthenticated == false)
            //        return Unauthorized();
            //    callerUserName = authRes.AuthenticatedUser;
            //    requestUserNameIsCaller = authRes.AuthenticatedUserEquals(userName);
            //}
            //catch (Exception authExc)
            //{
            //    return Unauthorized();
            //}

            User user = null;

            if (DbService.IsUser1BlockedByUser2(authenticatedUserName, userName))
                return NotFound();
            
            user = DbService.GetUser(userName, requestUserNameIsCaller);
            if(user != null)
            {
                user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 5);
                user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 5);
            }

            return Ok(ApiPayloadClass<User>.CreateSmallApiResponse(user));
        }

        [Route("SyncFromServer/{userLastModifiedOnClientString}")]
        [HttpGet]
        public async Task<IActionResult> SyncUser(string userLastModifiedOnClientString)
        {
            DateTime userLastModifiedOnClient = DateTime.ParseExact(userLastModifiedOnClientString, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            
            User user = null;

            user = DbService.GetUser(authenticatedUserName, true);
            if(user != null && user.LastModified > userLastModifiedOnClient)
            {
                user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 5);
                user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 5);
            }
            else
            {
                user = null;
            }

            //return Ok(new { Value = user });
            return Ok(ApiPayloadClass<User>.CreateSmallApiResponse(user));
        }


        [Route("{username}/HasCreatedProfile")]
        [HttpGet]
        public async Task<IActionResult> GetUserHasCreatedProfile(string username)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            bool result = DbService.GetUserHasCreatedProfile(authenticatedUserName);

            //return Ok(new { Value = result });
            return Ok(ApiPayloadClass<bool>.CreateSmallApiResponse(result));
        }

        [Route("{username}/HasCreatedProfile")]
        [HttpPut]
        public async Task<IActionResult> SetUserHasCreatedProfile(string username)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            DbService.SetUserHasCreatedProfile(authenticatedUserName);
            return Ok();
        }


        [Route("{username}/EditableProfile")]
        [HttpPut]
        public async Task<IActionResult> SetUserEditableProfile(string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            User user = null;
            try
            {
                user = await ApiPayloadClass<User>.GetRequestValueAsync(S3Client, Request.Body);
                if(user == null)
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
            
            if (!user.UserName.Equals(authenticatedUserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }
            User currentUserData = DbService.GetUser(authenticatedUserName, true);
            if(currentUserData == null)
                return BadRequest();

            string oldProfileImageKey = currentUserData.ProfilePictureUrl;
            string oldProfileImageKeyHighRes = currentUserData.ProfilePictureHighResUrl;

            if (user.ProfilePicture != null && user.ProfilePicture.Length > 0)
            {
                string uniqueFileName = string.Format(@"{0}_{1}.jpg", authenticatedUserName, Guid.NewGuid());
                string uniqueFileNameHighRes = string.Format(@"{0}_{1}.jpg", authenticatedUserName, Guid.NewGuid());

                using (MemoryStream imageStream = new MemoryStream(user.ProfilePicture))
                {
                    using (MemoryStream jpegStream = new MemoryStream())
                    {
                        Image.Load(imageStream).SaveAsJpeg(jpegStream);
                        user.ProfilePictureHighResUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileNameHighRes, jpegStream);
                    }

                    imageStream.Position = 0;
                    //Create Compressed Image
                    using (MemoryStream lowResJpegStream = new MemoryStream())
                    using (Image lowResImage = Image.Load(imageStream))
                    {
                        lowResImage.Mutate(x => x.Resize(300, 0));
                        lowResImage.SaveAsJpeg(lowResJpegStream, new JpegEncoder() { Quality = 75 });
                        user.ProfilePictureUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileName, lowResJpegStream);
                    }
                }

                //MemoryStream stream = new MemoryStream(user.ProfilePicture);

                //await S3Client.PutObjectAsync(FitAppS3Folder, uniqueFileName, stream);
            }
            else
            {
                user.ProfilePictureUrl = "";
                user.ProfilePictureHighResUrl = "";
            }
            

            DbService.EditUserProfile(user);

            if(user.PrimaryGym != null)
            {
                if(currentUserData.PrimaryGym == null || currentUserData.PrimaryGym.GroupId != user.PrimaryGym.GroupId)
                    DbService.UserSetPrimaryGym(authenticatedUserName, user.PrimaryGym.GroupId);
            }
            else
            {
                if(currentUserData.PrimaryGym != null)
                    DbService.UserRemovePrimaryGym(authenticatedUserName);
            }



            FireForgetRepositoryHandler.Execute(async (dbService, notifyService) =>
            {
                //User sets profil to public: all follow requests will get accepted
                if (currentUserData.IsPrivate && !user.IsPrivate)
                {
                    //get all pending followers
                    List<Follow> pendingFollowers = dbService.GetPendingFollowersFromUser(authenticatedUserName).ToList();

                    foreach (var pF in pendingFollowers)
                    {
                        //Sett all pending followers to follwing
                        int rowsaffected = dbService.UpdateFollowToAccepted(pF.Follower, pF.Following);

                        //Insert Notifications that the pending users are following now, but dont send notification
                        dbService.DeleteNotifications(pF.Follower, pF.Following, NotificationType.FollowRequest);
                        if (rowsaffected > 0)
                        {
                            await notifyService.SendNotification(pF.Follower, pF.Following, NotificationType.Follow, publish: false);
                        }   
                    }
                }
            });



            DbService.SetUserHasCreatedProfile(authenticatedUserName);

            if (!String.IsNullOrEmpty(oldProfileImageKey))
            {
                await S3Client.Delete(null, oldProfileImageKey);
            }
            if (!String.IsNullOrEmpty(oldProfileImageKeyHighRes))
            {
                await S3Client.Delete(null, oldProfileImageKeyHighRes);
            }

            return Ok();
        }


        [Route("Search")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string searchString)
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                if (String.IsNullOrEmpty(searchString) || searchString.Length > 128)
                    return BadRequest();

                List<User> users = DbService.GetUsersByUserNameOrFullName(searchString).ToList();
                List<BlockedUser> usersThatBlockedCaller = DbService.GetBlockingUsersFor(authenticatedUserName).ToList();
                List<User> resultUserList = new List<User>();
                foreach (User user in users)
                {
                    if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == user.UserName.ToLower()))
                        continue;
                    user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 5);
                    user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 5);
                    resultUserList.Add(user);
                }
               
                return Ok(ApiPayloadClass<List<User>>.CreateSmallApiResponse(resultUserList));
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [Route("S3UploadUrl")]
        [HttpGet]
        public async Task<IActionResult> GetS3UploadUrl()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            GetUploadPostUrlResult result = new GetUploadPostUrlResult();
             
            string uniqueFileName = string.Format(@"{0}_{1}_{2}.json", authenticatedUserName, "uploadReq", Guid.NewGuid());
            string filePath = S3Client.FitAppS3Folder + "/temp/" + uniqueFileName;
            result.Url = S3Client.GeneratePreSignedURL(filePath, HttpVerb.PUT, 24 * 60 * 7);
            result.FilePath = filePath;

            //return Ok(new { Value = result });
            return Ok(ApiPayloadClass<GetUploadPostUrlResult>.CreateSmallApiResponse(result));
        }

        [Route("Feedback")]
        [HttpPost]
        public async Task<IActionResult> SendFeedback([FromQuery]string subject, [FromQuery] string text)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            DbService.InsertFeedback(authenticatedUserName, subject, text);
            return Ok();
        }


        [Route("BlockedUsers/BiSync")]
        [HttpPost]
        public async Task<IActionResult> BlockedUsersBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();
            

            List<BlockedUser> clientBlockedUsers = null;
            DateTime? lastSync = null;
            SyncRequest<List<BlockedUser>> blockedUsersSyncReq = null;
            try
            {
                blockedUsersSyncReq = await ApiPayloadClass<SyncRequest<List<BlockedUser>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientBlockedUsers = blockedUsersSyncReq?.ObjectChangedOnClient;
                lastSync = blockedUsersSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

            if (clientBlockedUsers != null)
            {
                foreach (BlockedUser clientBlockedUser in clientBlockedUsers)
                {
                    try
                    {
                        if (clientBlockedUser.UserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        DbService.InsertOrUpdateBlockedUserIfNewer(clientBlockedUser);
                        if(clientBlockedUser.IsDeleted == false)
                        {
                            DbService.RemoveAllPostSubsFromUser1OnUser2(clientBlockedUser.BlockedUserName, clientBlockedUser.UserName);
                        }
                    }
                    catch (Exception ex2) { }
                }
            }

            //Get BlockedUsers with LastModified > lastSync
            List<BlockedUser> serverBlockedUsers = new List<BlockedUser>();
            if (lastSync == null)
            {
                serverBlockedUsers = DbService.GetAllBlockedUsersFromUserSinceDate(authenticatedUserName, DateTime.MinValue).ToList();
            }
            else
            {
                serverBlockedUsers = DbService.GetAllBlockedUsersFromUserSinceDate(authenticatedUserName, (DateTime)lastSync).ToList();
            }
            return Ok(await ApiPayloadClass<List<BlockedUser>>.CreateApiResponseAsync(S3Client, serverBlockedUsers));
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            try
            {
                await CognitoService.DisableUser(authenticatedUserName);

                DbService.DeleteUserWithFlag(authenticatedUserName);

                var userDevices = DbService.GetUserDevices(authenticatedUserName);
                foreach (var device in userDevices)
                {
                    DbService.DeleteUserDeviceEndpoint(authenticatedUserName, device.DeviceToken);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.ToString() });
            }
            

            return Ok();
        }


    }
}