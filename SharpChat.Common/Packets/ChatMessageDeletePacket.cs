using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageDeletePacket : ServerPacket {
        public long EventId { get; private set; }

        public ChatMessageDeletePacket(long eventId) {
            EventId = eventId;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageDelete);
            sb.Append('\t');
            sb.Append(EventId);

            yield return sb.ToString();
        }
    }
}
