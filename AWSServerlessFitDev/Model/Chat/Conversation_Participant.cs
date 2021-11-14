using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.Chat
{
    public class Conversation_Participant
    {
        public long ConversationId { get; set; }
        public string UserName { get; set; }

        public DateTime? ConvDeletedAt { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
