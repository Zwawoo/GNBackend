using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.Chat;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    //[Route("api/v1/[controller]")]
    //[ApiController]
    //public class TestController : Controller
    //{
    //    IDatabaseService DbService { get; set; }
    //    IS3Service S3Client { get; set; }
    //    ILogger<TestController> Logger { get; set; }
    //    INotificationService NotifyService { get; set; }
    //    private readonly IFireForgetRepositoryHandler FireForgetRepositoryHandler;

    //    public TestController(Services.IDatabaseService dbService, IConfiguration configuration, IS3Service s3Client, 
    //        INotificationService iNotifyService, IFireForgetRepositoryHandler fireForgetRepositoryHandler,
    //        ILogger<TestController> logger)
    //    {
    //        DbService = dbService;
    //        NotifyService = iNotifyService;
    //        S3Client = s3Client;
    //        Logger = logger;
    //        FireForgetRepositoryHandler = fireForgetRepositoryHandler;
    //    }
    //    //[AllowAnonymous]
    //    [Route("Test")]
    //    [HttpPost]
    //    public async Task<IActionResult> Test()
    //    {
    //        string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

    //        List<ChatMessage> clientChatMessages = null;
    //        DateTime? lastSync = null;

    //        try
    //        {
    //            SyncRequest<List<ChatMessage>> chatMessageSyncReq = await ApiPayloadClass<SyncRequest<List<ChatMessage>>>.GetRequestValueAsync(S3Client, Request.Body);
    //            clientChatMessages = chatMessageSyncReq?.ObjectChangedOnClient;
    //            lastSync = chatMessageSyncReq?.LastSyncTime;
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogException("loadtest", ex, Request);
    //            return BadRequest();
    //        }

    //        var serverFollows = (await DbService.GetAllFollowsFromUserSinceDate(authenticatedUserName, DateTime.MinValue))?.ToList();


    //        List<Post> serverPosts = (await DbService.GetPostsFromOwnUser(authenticatedUserName)).ToList();

    //        if (serverPosts != null)
    //        {
    //            foreach (Post post in serverPosts)
    //            {
    //                post.PostResourceUrl = S3Client.GeneratePreSignedURL(post.PostResourceUrl, HttpVerb.GET, 60);
    //                post.PostResourceThumbnailUrl = S3Client.GeneratePreSignedURL(post.PostResourceThumbnailUrl, HttpVerb.GET, 60);
    //            }
    //        }


    //        ChatMessageBiSyncResponse biSyncResponse = new ChatMessageBiSyncResponse();
    //        List<ChatMessage> newChatsOnServer = null;
    //        if (lastSync == null)
    //        {
    //            newChatsOnServer = (await DbService.GetChatMessagesforUserSinceDate("miron", DateTime.MinValue))?.ToList();
    //        }
    //        else
    //        {
    //            newChatsOnServer = (await DbService.GetChatMessagesforUserSinceDate("miron", (DateTime)lastSync))?.ToList();
    //        }
    //        if (newChatsOnServer != null)
    //        {
    //            foreach (var message in newChatsOnServer)
    //            {
    //                if (message.HasAttachment && message.Attachments != null && message.Attachments.Count > 0)
    //                {
    //                    message.Attachments[0].AttachmentUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentUrl, HttpVerb.GET, (7 * 60 * 24));
    //                    message.Attachments[0].AttachmentThumbnailUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
    //                }
    //            }
    //        }
    //        biSyncResponse.NewChatMessagesOnServer = newChatsOnServer;



    //        FireForgetRepositoryHandler.Execute(async (dbService, notifyService) =>
    //        {
    //            try
    //            {
    //                //User sets profil to public: all follow requests will get accepted
    //                    //get all pending followers
    //                    for(int i = 0; i <5; i++)
    //                    {
    //                        try
    //                        {
    //                        //Sett all pending followers to follwing
    //                            User u = await DbService.GetUser("miron", false);
    //                        }
    //                        catch (Exception ex2)
    //                        {
    //                            Logger?.LogError(ex2.ToString());
    //                        }
    //                    }
                    
    //            }
    //            catch (Exception ex)
    //            {
    //                Logger?.LogError(ex.ToString());
    //            }
    //        });

    //        return Ok(await ApiPayloadClass<ChatMessageBiSyncResponse>.CreateApiResponseAsync(S3Client, biSyncResponse));
    //    }
    //    //[AllowAnonymous]
    //    [Route("Test2")]
    //    [HttpGet]
    //    public async Task<IActionResult> GetNotificationTest2([FromQuery] string lastSyncString)
    //    {
    //        string authenticatedUserName = "miron";
    //        try
    //        {
    //            DateTime lastSyncTime = DateTime.ParseExact(lastSyncString, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

    //            List<Notification> notifications = (await DbService.GetNotifications(authenticatedUserName, lastSyncTime)).ToList();

    //            return Ok(await ApiPayloadClass<List<Notification>>.CreateApiResponseAsync(S3Client, notifications));
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.LogException(authenticatedUserName, ex, Request);
    //            return BadRequest();
    //        }
    //    }

    //}
}
