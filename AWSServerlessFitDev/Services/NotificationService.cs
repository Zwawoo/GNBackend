using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using AWSServerlessFitDev.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AWSServerlessFitDev.Services
{
    public class NotificationService : INotificationService
    {
        SNSService SnsService { get; set; }
        IDatabaseService DbService { get; set; }
        public NotificationService(IConfiguration configuration, IDatabaseService dbService)
        {
            //SnsService = new SNSService(configuration);
            DbService = dbService;
        }

        public async Task SendNotification(string userNameFrom, string userNameTo, NotificationType type, object content = null, long? postId_ = null, bool saveToDatabase = true, bool publish = true)
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
                                    Console.WriteLine("Device Token From " + userNameTo + " unregistered/invalid " + device.DeviceToken);
                                }

                            }
                            catch (Exception ex3)
                            {
                            }

                        }
                        catch (Exception ex2)
                        {
                        }
                    }
                }
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
