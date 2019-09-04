using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChatMessageDeletePacket : IServerPacket
    {
        public int EventId { get; private set; }

        public ChatMessageDeletePacket(int eventId)
        {
            EventId = eventId;
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageDelete);
            sb.Append(Constants.SEPARATOR);
            sb.Append(EventId);

            return new[] { sb.ToString() };
        }
    }
}
