using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders.Testing;
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
    public class FollowController : Controller
    {
        IDatabaseService DbService { get; set; }
        INotificationService NotifyService { get; set; }
        IS3Service S3Client { get; set; }
        ILogger<FollowController> Logger { get; set; }

        public FollowController(Services.IDatabaseService dbService, INotificationService iNotifyService, IS3Service s3Client, IConfiguration configuration,
            ILogger<FollowController> logger)
        {
            DbService = dbService;
            NotifyService = iNotifyService;
            S3Client = s3Client;
            Logger = logger;
        }


        [Route("BiSync")]
        [HttpPost]
        public async Task<IActionResult> FollowBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<Follow> clientFollows = null;
            DateTime? lastSync = null;

            try
            {
                SyncRequest<List<Follow>> followSyncReq = await ApiPayloadClass<SyncRequest<List<Follow>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientFollows = followSyncReq?.ObjectChangedOnClient;
                lastSync = followSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }


            if (clientFollows != null)
            {
                foreach (Follow clientFollow in clientFollows)
                {
                    try
                    {
                        if (!clientFollow.Follower.ToLower().Equals(authenticatedUserName.ToLower()))
                        { // Follower is foreign user, Thats the case if own User wants to delete a follower
                            if (clientFollow.Following.ToLower().Equals(authenticatedUserName.ToLower()))//Folowing User has to be the own username
                            {
                                if (clientFollow.IsDeleted)
                                {
                                    clientFollow.IsPending = false;
                                    int rowsaffected = await DbService.InsertOrReplaceFollowIfNewer(clientFollow);
                                    if (rowsaffected > 0)
                                    {
                                        await DbService.DeleteNotifications(clientFollow.Follower, authenticatedUserName, NotificationType.Follow);
                                        await DbService.DeleteNotifications(authenticatedUserName, clientFollow.Follower, NotificationType.FollowAccepted);
                                        await NotifyService.SendNotification(authenticatedUserName, clientFollow.Follower, NotificationType.FollowRemoved, saveToDatabase: false);
                                    }
                                }
                            }
                        }
                        else // Follower is own User
                        {
                            if (await DbService.IsUser1BlockedByUser2(authenticatedUserName, clientFollow.Following))
                                continue;

                            User userToFollow = await DbService.GetUser(clientFollow.Following, false);
                            if (userToFollow == null)
                                continue;

                            //Check if followrequest is not older than the user creation date (User deleted & new User with same username)
                            if (userToFollow.CreatedAt > clientFollow.LastModified)
                                continue;

                            if (clientFollow.IsDeleted)
                            {
                                clientFollow.IsPending = false;
                                int rowsaffected = await DbService.InsertOrReplaceFollowIfNewer(clientFollow);
                                if (rowsaffected > 0)
                                {
                                    await DbService.DeleteNotifications(authenticatedUserName, userToFollow.UserName, NotificationType.Follow);
                                    await DbService.DeleteNotifications(authenticatedUserName, userToFollow.UserName, NotificationType.FollowRequest);
                                    await NotifyService.SendNotification(authenticatedUserName, userToFollow.UserName, NotificationType.Unfollow, saveToDatabase: false);
                                }
                            }
                            else
                            {
                                //User to Follow is public
                                if (!userToFollow.IsPrivate)
                                {
                                    clientFollow.IsPending = false;
                                    int rowsaffected = await DbService.InsertOrReplaceFollowIfNewer(clientFollow);

                                    if (rowsaffected > 0)
                                        await NotifyService.SendNotification(authenticatedUserName, userToFollow.UserName, NotificationType.Follow);
                                }
                                else//User to Follow is private
                                {
                                    clientFollow.IsPending = true;
                                    int rowsaffected = await DbService.InsertOrReplaceFollowIfNewer(clientFollow);

                                    if (rowsaffected > 0)
                                        await NotifyService.SendNotification(authenticatedUserName, userToFollow.UserName, NotificationType.FollowRequest);
                                }

                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.LogException(authenticatedUserName, ex, Request);
                    } 
                }
            }

            //Get Follows related to User with LastModified >= lastSync
            List<Follow> serverFollows = new List<Follow>();
            if (lastSync == null)
            {
                serverFollows = (await DbService.GetAllFollowsFromUserSinceDate(authenticatedUserName, DateTime.MinValue))?.ToList();
                serverFollows.AddRange((await DbService.GetAllFollowersFromUserSinceDate(authenticatedUserName, DateTime.MinValue))?.ToList());
            }
            else
            {
                serverFollows = (await DbService.GetAllFollowsFromUserSinceDate(authenticatedUserName, (DateTime)lastSync))?.ToList();
                serverFollows.AddRange((await DbService.GetAllFollowersFromUserSinceDate(authenticatedUserName, (DateTime)lastSync))?.ToList());
            }

            //return Ok(new { Value = serverFollows });
            return Ok(await ApiPayloadClass<List<Follow>>.CreateApiResponseAsync(S3Client, serverFollows));
        }


        [Route("Accept/{userName}")]
        [HttpPut]
        public async Task<IActionResult> AcceptFollowRequest(string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            int rowsaffected = await DbService.UpdateFollowToAccepted(userName, authenticatedUserName);
            await DbService.DeleteNotifications(userName, authenticatedUserName, NotificationType.FollowRequest);
            if (rowsaffected > 0) //testen!!
            {
                await NotifyService.SendNotification(userName, authenticatedUserName, NotificationType.Follow, publish: false);
                await NotifyService.SendNotification(authenticatedUserName, userName, NotificationType.FollowAccepted);
            }
                
            return Ok();
        }


        [Route("Deny/{userName}")]
        [HttpPut]
        public async Task<IActionResult> DenyFollowRequest(string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            await DbService.UpdateFollowToDenied(userName, authenticatedUserName);
            await DbService.DeleteNotifications(userName, authenticatedUserName, NotificationType.FollowRequest);

            return Ok();
        }


        //[Route("Follows/{userName}/{offsetOldestUserName}/{limit:int}")]
        //[HttpGet]
        //public async Task<IActionResult> GetFollowsFromUser(string userName, string offsetOldestUserName, int limit)
        [Route("Follows/{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetFollowsFromUser([FromRoute] string userName, [FromQuery] string offsetOldestUserName, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            if (limit < 0 || limit > 50)
                return BadRequest();

            User requestedUser = await DbService.GetUser(userName, false);

            if (requestedUser == null || requestedUser.IsAboCountHidden)
                return BadRequest();
            if (requestedUser.IsPrivate)
            {
                if (!userName.ToLower().Equals(authenticatedUserName.ToLower()))
                {
                    if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, requestedUser.UserName))
                    {
                        return Unauthorized();
                    }
                }


            }
            List<User> users = new List<User>();

            users = (await DbService.GetUserFollowedByUser(userName, offsetOldestUserName, limit))?.ToList();
            if (users != null)
            {
                foreach (User user in users)
                {
                    user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 60 * 24);
                    user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 60 * 24);
                }
            }
            //return Ok(new { Value = users });
            return Ok(ApiPayloadClass<List<User>>.CreateSmallApiResponse(users));
        }

        //[Route("Follower/{userName}/{offsetOldestUserName}/{limit:int}")]
        //[HttpGet]
        //public async Task<IActionResult> GetFollowerFromUser(string userName, string offsetOldestUserName, int limit)
        [Route("Follower/{userName}")]
        [HttpGet]
        public async Task<IActionResult> GetFollowerFromUser([FromRoute] string userName, [FromQuery] string offsetOldestUserName, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            if (limit < 0 || limit > 50)
                return BadRequest();

            User requestedUser = await DbService.GetUser(userName, false);

            if (requestedUser == null || requestedUser.IsAboCountHidden)
                return BadRequest();
            if (requestedUser.IsPrivate)
            {
                if (!userName.ToLower().Equals(authenticatedUserName.ToLower()))
                {
                    if (!await DbService.IsUser1FollowingUser2(authenticatedUserName, requestedUser.UserName))
                    {
                        return Unauthorized();
                    }
                }
            }
            List<User> users = new List<User>();

            users = (await DbService.GetFollowerFromUser(userName, offsetOldestUserName, limit))?.ToList();
            if (users != null)
            {
                foreach (User user in users)
                {
                    user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 60 * 24);
                    user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 60 * 24);
                }
            }
            //return Ok(new { Value = users });
            return Ok(ApiPayloadClass<List<User>>.CreateSmallApiResponse(users));
        }


        //[Route("test")]
        //[HttpPut]
        //public async Task test([FromQuery] int isDeleted)
        //{
        //     var res = await DbService.InsertOrReplaceFollowIfNewer(new Follow()
        //    {
        //        Follower = "miron2",
        //        Following = "miron",
        //        IsDeleted = isDeleted == 1 ? true : false,
        //        IsPending = false,
        //        LastModified = DateTime.UtcNow
        //    });
        //}

    }
}
