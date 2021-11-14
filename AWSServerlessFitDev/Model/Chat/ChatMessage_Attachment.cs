using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.Chat
{
    public class ChatMessage_Attachment
    {
        public Guid AttachmentId { get; set; }
        public Guid ChatMessageId { get; set; }
        public AttachmentType AttachmentType { get; set; }
        public string AttachmentUrl { get; set; }
        public string AttachmentThumbnailUrl { get; set; }
        public byte[] Resource { get; set; }
        public byte[] ThumbnailResource { get; set; }
    }
}
