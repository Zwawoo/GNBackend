using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{

    public class NotificationDataRootObject
    {
        public string @default{ get;set;}

        public string APNS{get;set;}

        public string GCM {get; set;}

        public NotificationDataRootObject(long id, NotificationType type, string from, string to, object content, long postId)
        {
            @default = "not available";

            //APNS apns = new APNS()
            //{
            //    aps = new Aps()
            //    {
            //        alert = "Check out these awesome deals!",
            //        url = "www.amazon.com"
            //    }

            //};

            FCM fcm = new FCM()
            {
                data = new FCMData()
                {
                    Id = id,
                    FromUserName = from,
                    ToUserName = to,
                    NotificationTypeId = type,
                    PostId = postId,
                    TimeIssued = DateTime.UtcNow,
                    Content = content
                    
                }

            };

            //this.APNS = JsonConvert.SerializeObject(apns);

            this.GCM = JsonConvert.SerializeObject(fcm);

        }

    }

    public class FCM
    {
        public FCMData data { get; set; }
    }

    public class FCMData
    {
        public long Id { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public NotificationType NotificationTypeId { get; set; }
        public long PostId { get; set; }
        public DateTime TimeIssued { get; set; }
        public object Content { get; set; }
    }

    public class APNS
    {
        public Aps aps { get; set; }
    }
    public class Aps
    {
        public long Id { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public NotificationType NotificationTypeId { get; set; }
        public long PostId { get; set; }
        public DateTime TimeIssued { get; set; }
        public object Content { get; set; }
    }

    
}
