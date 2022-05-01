using System;

namespace AWSServerlessFitDev.Model
{
    public class NotificationSetting
    {
        public NotificationType NotificationType { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime LastModified { get; set; }
    }
}
