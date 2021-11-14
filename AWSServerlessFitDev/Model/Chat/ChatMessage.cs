using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.Chat
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; }
        public string FromUserName { get; set; }
        public long? ConversationId { get; set; }

        // Set if no Conversation is created on Backend yet
        public string ToUserName { get; set; }

        public string Text{get; set;}
        public DateTime? CreatedOnClientAt { get; set; }
        public DateTime? CreatedOnServerAt { get; set; }


        public bool HasAttachment { get; set; }
        public List<ChatMessage_Attachment> Attachments { get; set; }

    }
}
