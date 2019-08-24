using System;
using System.Text;

namespace SharpChat
{
    public class SockChatMessage : IChatMessage
    {
        public static int MessageIdCounter { get; private set; } = 0;
        public static int NextMessageId => ++MessageIdCounter;

        public int MessageId { get; set; }
        public SockChatUser User { get; set; }
        public SockChatChannel Channel { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public MessageFlags Flags { get; set; } = MessageFlags.RegularUser;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(DateTime.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(User);
            sb.Append('\t');
            sb.Append(Text);
            sb.Append('\t');
            sb.Append(MessageId);
            sb.Append("\t0\t");
            sb.Append(Flags.Serialise());

            return sb.ToString();
        }

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
