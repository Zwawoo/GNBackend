using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.Chat
{
    public class Conversation
    {
        public long? ConversationId { get; set; }
        public bool IsDirectChat { get; set; }
        public string GroupImageURI { get; set; }
        public string GroupName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }

        public string DirectChatOtherUserName { get; set; }
        public Guid DirectChatOtherUserSubId { get; set; }

    }
}
