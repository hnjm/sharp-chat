using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class ChannelDeletePacket : ServerPacket {
        public ChatChannel Channel { get; private set; }

        public ChannelDeletePacket(ChatChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append('\t');
            sb.Append((int)SockChatServerChannelPacket.Delete);
            sb.Append('\t');
            sb.Append(Channel.Name);

            yield return sb.ToString();
        }
    }
}
