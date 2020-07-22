using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class ChannelUpdatePacket : ServerPacket {
        public string PreviousName { get; private set; }
        public ChatChannel Channel { get; private set; }

        public ChannelUpdatePacket(string previousName, ChatChannel channel) {
            PreviousName = previousName;
            Channel = channel;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append('\t');
            sb.Append((int)SockChatServerChannelPacket.Update);
            sb.Append('\t');
            sb.Append(PreviousName);
            sb.Append('\t');
            sb.Append(Channel.Pack());

            yield return sb.ToString();
        }
    }
}
