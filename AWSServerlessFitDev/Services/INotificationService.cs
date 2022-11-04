using AWSServerlessFitDev.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public interface INotificationService
    {
        Task SendNotification(string userNameFrom, string userNameTo, NotificationType type, object content = null, long? postId = null, bool saveToDatabase = true, bool publish = true, long? commentId = null);
        Task SendAlertNotification(string userNameTo, string text, NotificationType type);
    }
}
