using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageDeletePacket : ServerPacketBase {
        public long EventId { get; private set; }

        public ChatMessageDeletePacket(long eventId) {
            EventId = eventId;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageDelete);
            sb.Append('\t');
            sb.Append(EventId);

            yield return sb.ToString();
        }
    }
}
