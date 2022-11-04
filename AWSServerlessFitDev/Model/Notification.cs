using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Notification
    {
        public long Id { get; set; }
        public string FromUserName { get; set; }
        //public Guid FromUserSubId { get; set; }
        public string ToUserName { get; set; }
        //public Guid ToUserSubId { get; set; }
        public NotificationType NotificationTypeId { get; set; }
        public long PostId { get; set; }
        public long? PostCommentId { get; set; }
        public DateTime TimeIssued { get; set; }

        //public string CreateJsonMessage()
        //{
        //    return JsonConvert.SerializeObject(new NotificationDataRootObject(Id, NotificationTypeId, FromUserName, ToUserName, PostId));
        //}
    }
}
