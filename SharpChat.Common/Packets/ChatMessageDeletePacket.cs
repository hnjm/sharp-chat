using SharpChat.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChatMessageDeletePacket : ServerPacketBase {
        public IEvent Event { get; private set; }

        public ChatMessageDeletePacket(IEvent evt) {
            Event = evt ?? throw new ArgumentNullException(nameof(evt));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageDelete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Event.EventId);

            yield return sb.ToString();
        }
    }
}
