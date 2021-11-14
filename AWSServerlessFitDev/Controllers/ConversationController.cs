using Amazon.CognitoIdentityProvider.Model;
using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.Chat;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConversationController : Controller
    {
        IDatabaseService DbService { get; set; }
        INotificationService NotifyService { get; set; }
        S3Service S3Client { get; set; }
        public ConversationController(Services.IDatabaseService dbService, INotificationService iNotifyService, IAmazonS3 s3Client, IConfiguration configuration)
        {
            DbService = dbService;
            NotifyService = iNotifyService;
            S3Client = new S3Service(configuration, s3Client);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations([FromQuery] string lastSync)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            DateTime lastSyncTime = DateTime.ParseExact(lastSync, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            List<Conversation> newOrUpdatedConvs = DbService.GetNewOrUpdatedConversations(authenticatedUserName, lastSyncTime).ToList();
            foreach (var conv in newOrUpdatedConvs)
            {
                if (conv.IsDirectChat)
                {
                    List<Conversation_Participant> convParts = DbService.GetConversationParticipants((long)conv.ConversationId).ToList();
                    conv.DirectChatOtherUserName = convParts.FirstOrDefault(x => x.UserName.ToLower() != authenticatedUserName.ToLower()).UserName;
                }
            }
            return Ok(ApiPayloadClass<List<Conversation>>.CreateSmallApiResponse(newOrUpdatedConvs));
        }

        [Route("ConversationParticipants/BiSync")]
        [HttpPost]
        public async Task<IActionResult> BiSyncConversationParticipants()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<Conversation_Participant> clientCPs = null;
            DateTime? lastSync = null;

            try
            {
                SyncRequest<List<Conversation_Participant>> chatMessageSyncReq = await ApiPayloadClass<SyncRequest<List<Conversation_Participant>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientCPs = chatMessageSyncReq?.ObjectChangedOnClient;
                lastSync = chatMessageSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

            foreach (var cP in clientCPs)
            {
                if (authenticatedUserName.ToLower() == cP.UserName.ToLower())
                {
                    DbService.UpdateConversationParticipantIfNewer(cP);
                }

            }
            if (lastSync == null)
                lastSync = default(DateTime);
            List<Conversation_Participant> cPChangedOnServer = DbService.GetNewOrUpdatedConversationParticipants(authenticatedUserName, lastSync.Value).ToList();
            return Ok(ApiPayloadClass<List<Conversation_Participant>>.CreateSmallApiResponse(cPChangedOnServer));
        }

        [Route("ChatMessages/BiSync")]
        [HttpPost]
        public async Task<IActionResult> ChatMessagesBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            List<ChatMessage> clientChatMessages = null;
            DateTime? lastSync = null;

            try
            {
                SyncRequest<List<ChatMessage>> chatMessageSyncReq = await ApiPayloadClass<SyncRequest<List<ChatMessage>>>.GetRequestValueAsync(S3Client, Request.Body);
                clientChatMessages = chatMessageSyncReq?.ObjectChangedOnClient;
                lastSync = chatMessageSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

            ChatMessageBiSyncResponse biSyncResponse = new ChatMessageBiSyncResponse();
            biSyncResponse.ClientChatMessagesFailedToSend = new List<Guid>();

            if (clientChatMessages != null)
            {
                List<BlockedUser> usersThatBlockedCaller = DbService.GetBlockingUsersFor(authenticatedUserName).ToList();
                List<BlockedUser> blockedUsersByCaller = DbService.GetAllBlockedUsersFromUserSinceDate(authenticatedUserName, Constants.MysqlMinDateTime).Where(bu => bu.IsDeleted == false).ToList();
                clientChatMessages = clientChatMessages.OrderBy(x => x.CreatedOnClientAt).ToList();
                foreach (var chatMessage in clientChatMessages)
                {
                    try
                    {
                        if (chatMessage.FromUserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        if (chatMessage.ConversationId == null)
                        {
                            if (!String.IsNullOrWhiteSpace(chatMessage.ToUserName))
                            {
                                long? convId = DbService.CreateDirectConversationIfNotExist(authenticatedUserName, chatMessage.ToUserName);
                                if (convId == null)
                                    continue;
                                chatMessage.ConversationId = convId;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        //Is user allowed to send message to conversation
                        List<Conversation_Participant> participants = DbService.GetConversationParticipants(chatMessage.ConversationId.Value).ToList();
                        if (!participants.Any(x => x.UserName.ToLower() == authenticatedUserName.ToLower()))
                            continue;

                        //Check if recipient blocked the sender 
                        // Needs rework if group chats are implemented
                        Conversation_Participant recipient = participants.FirstOrDefault(u => u.UserName.ToLower() != authenticatedUserName.ToLower());
                        //if (DbService.IsUser1BlockedByUser2(userName, recipient.UserName))
                        //    continue;
                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == recipient.UserName))
                            continue;
                        //Check if Sender has blocked the recipient
                        if (blockedUsersByCaller.Any(u => u.BlockedUserName.ToLower() == recipient.UserName))
                            continue;

                        chatMessage.CreatedOnServerAt = DateTime.UtcNow;
                        int affectedRows = DbService.InsertOrIgnoreChatMessage(chatMessage);
                        //affectedRows > 0 means that the message is inserted. If it is 0, the message already exists
                        if (affectedRows > 0)
                        {
                            foreach (var participant in participants)
                            {
                                try
                                {
                                    if (participant.UserName.ToLower() == authenticatedUserName.ToLower())
                                        continue;
                                    await NotifyService.SendNotification(authenticatedUserName, participant.UserName, NotificationType.ChatMessage, content: chatMessage);
                                }
                                catch (Exception sendEx)
                                {

                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                        biSyncResponse.ClientChatMessagesFailedToSend.Add(chatMessage.MessageId);
                    }


                }
            }

            List<ChatMessage> newChatsOnServer = null;
            if (lastSync == null)
            {
                newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(authenticatedUserName, DateTime.MinValue)?.ToList();
            }
            else
            {
                newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(authenticatedUserName, (DateTime)lastSync)?.ToList();
            }
            if (newChatsOnServer != null)
            {
                foreach (var message in newChatsOnServer)
                {
                    if (message.HasAttachment && message.Attachments != null && message.Attachments.Count > 0)
                    {
                        message.Attachments[0].AttachmentUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentUrl, HttpVerb.GET, (7 * 60 * 24));
                        message.Attachments[0].AttachmentThumbnailUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
                    }
                }
            }
            biSyncResponse.NewChatMessagesOnServer = newChatsOnServer;
            return Ok(await ApiPayloadClass<ChatMessageBiSyncResponse>.CreateApiResponseAsync(S3Client, biSyncResponse));
        }



        [Route("ChatMessages/WithAttachment")]
        [HttpPost]
        public async Task<IActionResult> SendChatMessageWithAttachment()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            ChatMessage chatMessage = null;
            try
            {
                chatMessage = await ApiPayloadClass<ChatMessage>.GetRequestValueAsync(S3Client, Request.Body);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

            if (chatMessage != null)
            {
                if (chatMessage.FromUserName.ToLower() != authenticatedUserName.ToLower())
                    return BadRequest();
                if (chatMessage.ConversationId == null)
                {
                    if (!String.IsNullOrWhiteSpace(chatMessage.ToUserName))
                    {
                        long? convId = DbService.CreateDirectConversationIfNotExist(authenticatedUserName, chatMessage.ToUserName);
                        if (convId == null)
                            return BadRequest();
                        chatMessage.ConversationId = convId;
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                //Is user allowed to send message to conversation
                List<Conversation_Participant> participants = DbService.GetConversationParticipants(chatMessage.ConversationId.Value).ToList();
                if (!participants.Any(x => x.UserName.ToLower() == authenticatedUserName.ToLower()))
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);

                //Check if recipient blocked the sender 
                // Needs rework if group chats are implemented
                Conversation_Participant recipient = participants.FirstOrDefault(u => u.UserName.ToLower() != authenticatedUserName.ToLower());
                if (DbService.IsUser1BlockedByUser2(authenticatedUserName, recipient.UserName))
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                //Check if Sender has blocked the recipient
                if (DbService.IsUser1BlockedByUser2(recipient.UserName, authenticatedUserName))
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);

                chatMessage.CreatedOnServerAt = DateTime.UtcNow;

                try
                {
                    //DbService.InsertOrIgnoreChatMessage(chatMessage);
                    foreach (var attachment in chatMessage.Attachments)
                    {
                        if (attachment.ChatMessageId == chatMessage.MessageId && !String.IsNullOrEmpty(chatMessage.MessageId.ToString()))
                        {
                            string fileEnding = attachment.AttachmentType == AttachmentType.Image ? "jpg" : "mp4";
                            string uniqueFileName = string.Format(@"{0}_{1}_{2}.{3}", chatMessage.ConversationId, chatMessage.FromUserName, Guid.NewGuid(), fileEnding);
                            string uniqueFileNameThumbnail = string.Format(@"{0}_{1}_{2}.jpg", chatMessage.ConversationId, chatMessage.FromUserName, Guid.NewGuid());
                            if (attachment.AttachmentType == AttachmentType.Image)
                            {
                                if (!Utils.IsValidImage(attachment.Resource))
                                {
                                    return BadRequest();
                                }

                                const string format = "yyyy:MM:dd HH:mm:ss";
                                var now = DateTime.Now;
                                var dt = now.ToString(format);

                                using (MemoryStream stream = new MemoryStream(attachment.Resource))
                                {
                                    using (MemoryStream jpegStream = new MemoryStream())
                                    using (Image img = Image.Load(stream))
                                    {
                                        //try
                                        //{
                                        //    img.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTime, dt);
                                        //    img.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, dt);
                                        //    img.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeDigitized, dt);
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //}

                                        img.SaveAsJpeg(jpegStream);
                                        attachment.AttachmentUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileName, jpegStream);
                                    }

                                    stream.Position = 0;
                                    //Create Compressed Image
                                    using (MemoryStream lowResJpegStream = new MemoryStream())
                                    using (Image lowResImage = Image.Load(stream))
                                    {
                                        lowResImage.Mutate(x => x.Resize(200, 0));
                                        //try
                                        //{
                                        //    lowResImage.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTime, dt);
                                        //    lowResImage.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, dt);
                                        //    lowResImage.Metadata.ExifProfile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeDigitized, dt);
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //}
                                        lowResImage.SaveAsJpeg(lowResJpegStream, new JpegEncoder() { Quality = 50 });
                                        attachment.AttachmentThumbnailUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileNameThumbnail, lowResJpegStream);
                                    }
                                }

                            }
                            else if (attachment.AttachmentType == AttachmentType.Video)
                            {
                                if (attachment.ThumbnailResource == null || attachment.ThumbnailResource.Length < 1)
                                {
                                    return BadRequest();
                                }

                                using (MemoryStream videoStream = new MemoryStream(attachment.Resource))
                                {
                                    attachment.AttachmentUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileName, videoStream);
                                }
                                using (MemoryStream thumbnailStream = new MemoryStream(attachment.ThumbnailResource))
                                {
                                    attachment.AttachmentThumbnailUrl = await S3Client.PutObjectAsync(S3Client.FitAppS3Folder, uniqueFileNameThumbnail, thumbnailStream);
                                }
                            }

                            //DbService.InsertOrIgnoreAttachment(attachment);
                            attachment.Resource = null;
                            attachment.ThumbnailResource = null;

                        }
                    }

                    int affectedRows = DbService.InsertOrIgnoreChatMessageWithAttachments(chatMessage);
                    if (affectedRows > 0)
                    {
                        foreach (var participant in participants)
                        {
                            if (participant.UserName.ToLower() == authenticatedUserName.ToLower())
                                continue;
                            await NotifyService.SendNotification(authenticatedUserName, participant.UserName, NotificationType.ChatMessage, content: chatMessage);
                        }
                    }

                }
                catch (Exception ex)
                {
                    //return UnprocessableEntity();
                    throw;
                }
            }
            return Ok();
        }

        [Route("Attachment/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetAttachment([FromRoute] Guid id)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            ChatMessage_Attachment attachment = DbService.GetChatMessageAttachment(id);
            if (attachment == null)
                return BadRequest();

            ChatMessage msg = DbService.GetChatMessage(attachment.ChatMessageId);
            if (msg == null)
                return BadRequest();

            List<Conversation_Participant> participants = DbService.GetConversationParticipants((long)msg.ConversationId).ToList();

            if (participants.Any(x => x.UserName.ToLower() == authenticatedUserName.ToLower()))
            {
                attachment.AttachmentUrl = S3Client.GeneratePreSignedURL(attachment.AttachmentUrl, HttpVerb.GET, (7 * 60 * 24));
                attachment.AttachmentThumbnailUrl = S3Client.GeneratePreSignedURL(attachment.AttachmentThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
                return Ok(ApiPayloadClass<ChatMessage_Attachment>.CreateSmallApiResponse(attachment));
            }
            else
                return Unauthorized();

        }







        //[Obsolete]
        //[Route("ChatMessages/WithoutAttachment/BiSync")]
        //[HttpPost]
        //public async Task<IActionResult> ChatMessagesWithoutAttachmentBiSync()
        //{
        //    string userName = "";
        //    try
        //    {
        //        AuthenticationResult authRes = await Utils.CheckAuthentication(Request);
        //        if (authRes.IsAuthenticated == false)
        //            return Unauthorized();
        //        userName = authRes.AuthenticatedUser;
        //    }
        //    catch (Exception authExc)
        //    {
        //        return Unauthorized();
        //    }

        //    List<ChatMessage> clientChatMessages = null;
        //    DateTime? lastSync = null;

        //    try
        //    {
        //        SyncRequest<List<ChatMessage>> chatMessageSyncReq = await ApiPayloadClass<SyncRequest<List<ChatMessage>>>.GetRequestValueAsync(S3Client, Request.Body);
        //        clientChatMessages = chatMessageSyncReq?.ObjectChangedOnClient;
        //        lastSync = chatMessageSyncReq?.LastSyncTime;
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest();
        //    }

        //    ChatMessageBiSyncResponse biSyncResponse = new ChatMessageBiSyncResponse();
        //    biSyncResponse.ClientChatMessagesFailedToSend = new List<Guid>();

        //    if (clientChatMessages != null)
        //    {
        //        clientChatMessages = clientChatMessages.OrderBy(x => x.CreatedOnClientAt).ToList();
        //        foreach (var chatMessage in clientChatMessages)
        //        {
        //            try
        //            {
        //                if (chatMessage.FromUserName.ToLower() != userName.ToLower())
        //                    return BadRequest();
        //                if (chatMessage.ConversationId == null)
        //                {
        //                    if (!String.IsNullOrWhiteSpace(chatMessage.ToUserName))
        //                    {
        //                        long? convId = DbService.CreateDirectConversationIfNotExist(userName, chatMessage.ToUserName);
        //                        if (convId == null)
        //                            return BadRequest();
        //                        chatMessage.ConversationId = convId;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //                //Is user allowed to send message to conversation
        //                List<Conversation_Participant> participants = DbService.GetConversationParticipants(chatMessage.ConversationId.Value).ToList();
        //                if (!participants.Any(x => x.UserName.ToLower() == userName.ToLower()))
        //                    continue;

        //                chatMessage.CreatedOnServerAt = DateTime.UtcNow;
        //                int affectedRows = DbService.InsertOrIgnoreChatMessage(chatMessage);
        //                //affectedRows > 0 means that the message is inserted. If it is 0, the message already exists
        //                if (affectedRows > 0)
        //                {
        //                    foreach (var participant in participants)
        //                    {
        //                        try
        //                        {
        //                            if (participant.UserName.ToLower() == userName.ToLower())
        //                                continue;
        //                            await NotifyService.SendNotification(userName, participant.UserName, NotificationType.ChatMessage, content: chatMessage);
        //                        }
        //                        catch (Exception sendEx)
        //                        {

        //                        }

        //                    }
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                System.Console.WriteLine(ex.ToString());
        //                biSyncResponse.ClientChatMessagesFailedToSend.Add(chatMessage.MessageId);
        //            }


        //        }
        //    }

        //    List<ChatMessage> newChatsOnServer = null;
        //    if (lastSync == null)
        //    {
        //        newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(userName, DateTime.MinValue, false)?.ToList();
        //    }
        //    else
        //    {
        //        newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(userName, (DateTime)lastSync, false)?.ToList();
        //    }
        //    biSyncResponse.NewChatMessagesOnServer = newChatsOnServer;
        //    return Ok(await ApiPayloadClass<ChatMessageBiSyncResponse>.CreateApiResponseAsync(S3Client, biSyncResponse));
        //}

        //[Obsolete]
        //[Route("ChatMessages/WithAttachment")]
        //[HttpGet]
        //public async Task<IActionResult> GetChatMessagesWithAttachment([FromQuery] string lastSyncString)
        //{
        //    string userName = "";
        //    try
        //    {
        //        AuthenticationResult authRes = await Utils.CheckAuthentication(Request);
        //        if (authRes.IsAuthenticated == false)
        //            return Unauthorized();
        //        userName = authRes.AuthenticatedUser;
        //    }
        //    catch (Exception authExc)
        //    {
        //        return Unauthorized();
        //    }
        //    DateTime lastSync = DateTime.ParseExact(lastSyncString, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

        //    List<ChatMessage> newChatsOnServer = null;
        //    if (lastSync == null)
        //    {
        //        newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(userName, DateTime.MinValue, true)?.ToList();
        //    }
        //    else
        //    {
        //        newChatsOnServer = DbService.GetChatMessagesforUserSinceDate(userName, (DateTime)lastSync, true)?.ToList();
        //    }
        //    if (newChatsOnServer != null)
        //    {
        //        foreach (var message in newChatsOnServer)
        //        {
        //            if (message.Attachments != null && message.Attachments.Count > 0)
        //            {
        //                message.Attachments[0].AttachmentUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentUrl, HttpVerb.GET, (7 * 60 * 24));
        //                message.Attachments[0].AttachmentThumbnailUrl = S3Client.GeneratePreSignedURL(message.Attachments[0].AttachmentThumbnailUrl, HttpVerb.GET, (7 * 60 * 24));
        //            }
        //        }
        //    }
        //    return Ok(await ApiPayloadClass<List<ChatMessage>>.CreateApiResponseAsync(S3Client, newChatsOnServer));
        //}


        //[Route("test")]
        //[HttpGet]
        //public async Task<IActionResult> test()
        //{
        //    //var newChatsOnServer = DbService.GetChatMessagesforUserSinceDate("miron", DateTime.MinValue)?.ToList();
        //    try
        //    {
        //        throw new ArrayTypeMismatchException("More Information");
        //    }
        //    catch(Exception ex)
        //    {

        //    }

        //    return Ok();
        //}


        //[Route("test")]
        //[HttpPut]
        //public async Task test()
        //{
        //    ChatMessage cm = new ChatMessage()
        //    {
        //        ConversationId = 18,
        //        CreatedOnServerAt = DateTime.UtcNow,
        //        FromUserName = "miron",
        //        MessageId = Guid.NewGuid(),
        //        Text = "manTest", ToUserName = "timfit"
        //    };
        //    await NotifyService.SendNotification("miron", "timfit", NotificationType.ChatMessage, content: cm);
        //}


    }
}
