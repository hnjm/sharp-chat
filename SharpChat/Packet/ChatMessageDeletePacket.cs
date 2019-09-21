using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChatMessageDeletePacket : ServerPacket
    {
        public int EventId { get; private set; }

        public ChatMessageDeletePacket(int eventId)
        {
            EventId = eventId;
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageDelete);
            sb.Append('\t');
            sb.Append(EventId);

            return new[] { sb.ToString() };
        }
    }
}
