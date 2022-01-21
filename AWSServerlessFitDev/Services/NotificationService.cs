using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using AWSServerlessFitDev.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AWSServerlessFitDev.Services
{
    public class NotificationService : INotificationService
    {
        SNSService SnsService { get; set; }
        IDatabaseService DbService { get; set; }
        ILogger<NotificationService> Logger { get; set; }
        public NotificationService(IConfiguration configuration, IDatabaseService dbService, ILogger<NotificationService> logger)
        {
            //SnsService = new SNSService(configuration);
            DbService = dbService;
            Logger = logger;
        }

        public async Task SendNotification(string userNameFrom, string userNameTo, NotificationType type, object content = null, long? postId_ = null, bool saveToDatabase = true, bool publish = true)
        {
            try
            {
                long postId = postId_ == null ? -1 : (long)postId_;
                long notificationId = -1;
                if (type != NotificationType.ChatMessage && saveToDatabase == true)
                {
                    notificationId = DbService.InsertNotification(new Notification()
                    {
                        FromUserName = userNameFrom,
                        ToUserName = userNameTo,
                        NotificationTypeId = type,
                        PostId = postId,
                        TimeIssued = DateTime.UtcNow
                    }) ?? -1;
                }

                if (publish)
                {
                    List<Device> userDevices = DbService.GetUserDevices(userNameTo)?.ToList();

                    if (userDevices != null && userDevices.Count > 0)
                    {
                        foreach (Device device in userDevices)
                        {
                            try
                            {
                                await FCMPublishMessage(device.DeviceToken, notificationId, type, userNameFrom, userNameTo, content, postId);
                            }
                            catch (FirebaseAdmin.Messaging.FirebaseMessagingException ex)
                            {
                                try
                                {
                                    //if(ex.ErrorCode == FirebaseAdmin.ErrorCode.Unavailable)
                                    //{

                                    //}
                                    //else if(ex.MessagingErrorCode == FirebaseAdmin.Messaging.MessagingErrorCode.Unavailable)
                                    //{

                                    //}
                                    if (ex.MessagingErrorCode == FirebaseAdmin.Messaging.MessagingErrorCode.Unregistered)
                                    {
                                        //delete device endpoint
                                        DbService.DeleteUserDeviceEndpoint(userNameTo, device.DeviceToken);
                                        Logger.LogWarning("Device Token From UserName={userName} is unregistered/invalid Token={token}", userNameTo, device.DeviceToken);
                                    }

                                }
                                catch (Exception ex3)
                                {
                                    Logger.LogError(ex3.ToString());
                                }

                            }
                            catch (Exception ex2)
                            {
                                Logger.LogError("Error publishing Notification. \n " +
                                                "Exception={exception} \n" +
                                                "UserNameFrom={userNameFrom} \n" +
                                                "UserNameTo={UserNameTo} \n" +
                                                "Type={type} \n" +
                                                "content={content} \n" +
                                                "PostId={postId} \n " +
                                                "Token={token}", ex2.ToString(), userNameFrom, userNameTo, type, content.ToString(), postId_, device.DeviceToken);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("Exception sending Notification. \n" +
                    "UserNameFrom={userNameFrom} \n" +
                    "UserNameTo={UserNameTo} \n" +
                    "Type={type} \n" +
                    "content={content} \n" +
                    "PostId={postId} \n " +
                    "saveToDatabase={saveToDatabase} publish={publish}", userNameFrom, userNameTo, type, content.ToString(), postId_, saveToDatabase, publish);
            }
            
        }

        public async Task FCMPublishMessage(string token, long id, NotificationType type, string from, string to, object content, long postId)
        {
            var data = new Dictionary<string, string>();
            data.Add("Id", id.ToString());
            data.Add("FromUserName", from);
            data.Add("ToUserName", to);
            data.Add("NotificationTypeId", ((int)type).ToString());
            data.Add("PostId", postId.ToString());
            data.Add("TimeIssued", DateTime.UtcNow.ToString("O"));
            data.Add("Content", JsonConvert.SerializeObject(content));


            var iOSHeaders = new Dictionary<string, string>();
            bool mutableContent = true;
            bool contentAvailable = false;
            //make ios push notification silent
            if(type == NotificationType.Unfollow || type == NotificationType.FollowRemoved || type == NotificationType.PostUnlike )
            {
                mutableContent = false;
                contentAvailable = true;
                iOSHeaders.Add("apns-push-type", "background");
            }
            else
            {
                iOSHeaders.Add("apns-push-type", "alert");
            }
            iOSHeaders.Add("apns-topic", "com.Gymnect.Gymnect");


            FirebaseAdmin.Messaging.Message msg = new FirebaseAdmin.Messaging.Message()
            {
                Token = token,
                Data = data,
                Apns = new FirebaseAdmin.Messaging.ApnsConfig()
                {
                    Aps = new FirebaseAdmin.Messaging.Aps()
                    {
                        Alert = new FirebaseAdmin.Messaging.ApsAlert()
                        {
                            Title = "Gymnect",
                            Body = "Neue Benachrichtigung"
                        },
                        ContentAvailable = contentAvailable, 
                        MutableContent = mutableContent, 
                        Sound = "default" 
                    }, 
                    Headers = iOSHeaders 
                }
            };
            string res = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(msg);
        }
        //public async Task SendNotification(string userNameFrom, string userNameTo, NotificationType type, object content = null, long? postId_ = null, bool saveToDatabase = true, bool publish = true)
        //{
        //    long postId = postId_ == null ? -1 : (long)postId_;
        //    long notificationId = -1;
        //    if (type != NotificationType.ChatMessage && saveToDatabase == true)
        //    {
        //        notificationId = DbService.InsertNotification(new Notification()
        //        {
        //            FromUserName = userNameFrom,
        //            ToUserName = userNameTo,
        //            NotificationTypeId = type,
        //            PostId = postId,
        //            TimeIssued = DateTime.UtcNow
        //        }) ?? -1;
        //    }

        //    if (publish)
        //    {
        //        List<Device> userDevices = DbService.GetUserDevices(userNameTo)?.ToList();

        //        if (userDevices != null && userDevices.Count > 0)
        //        {
        //            string message = JsonConvert.SerializeObject(new NotificationDataRootObject(notificationId, type, userNameFrom, userNameTo, content, postId));

        //            foreach (Device device in userDevices)
        //            {
        //                try
        //                {
        //                    await SnsService.PublishMessage(device.EndpointArn, message);
        //                }
        //                catch (EndpointDisabledException ex)
        //                {
        //                    try
        //                    {
        //                        DbService.DeleteUserDeviceEndpoint(userNameTo, device.DeviceToken);
        //                        if (device.DeviceType.ToLower() == "android")
        //                        {
        //                            //Console.WriteLine("Device deleted");
        //                            await SnsService.DeleteAndroidEndpoint(device.EndpointArn);
        //                        }
        //                    }
        //                    catch (Exception ex3)
        //                    {
        //                    }

        //                }
        //                catch (Exception ex2)
        //                {
        //                }
        //            }
        //        }
        //    }   
        //}



    }
}
