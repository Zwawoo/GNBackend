using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.Chat
{
    public class ChatMessageBiSyncResponse
    {
        public List<ChatMessage> NewChatMessagesOnServer { get; set; }
        public List<Guid> ClientChatMessagesFailedToSend { get; set; }
    }
}
