using System;
using System.Text;

namespace SharpChat
{
    public class SockChatMessage : IChatMessage
    {
        public int MessageId { get; set; }
        public SockChatUser User { get; set; }
        public SockChatChannel Channel { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public SockChatMessageFlags Flags { get; set; } = SockChatMessageFlags.RegularUser;

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
