using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public class SockChatMessage
    {
        public static int MessageIdCounter { get; private set; } = 0;
        public static string NextMessageId => (++MessageIdCounter).ToString();

        public int MessageId { get; set; }
        public SockChatUser User { get; set; }
        public SockChatChannel Channel { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }

        public string GetLogString()
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
            sb.Append(@"10010"); // BOLD CURSIVE UNDERLINED COLON PRIVATE

            return sb.ToString();
        }

        public static string PackBotMessage(int type, string id, params string[] parts)
        {
            return type.ToString() + '\f' + id + '\f' + string.Join('\f', parts);
        }
    }
}
