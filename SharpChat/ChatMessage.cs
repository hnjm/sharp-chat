using System;

namespace SharpChat
{
    public class ChatMessage : IChatMessage
    {
        public ChatUser Sender { get; set; }
        public IPacketTarget Target { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public ChatMessageFlags Flags { get; set; } = ChatMessageFlags.None;
        public int SequenceId { get; set; }

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
