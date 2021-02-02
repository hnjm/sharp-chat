using SharpChat.Channels;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelUpdatePacket : ServerPacketBase {
        public string PreviousName { get; private set; }
        public Channel Channel { get; private set; }

        public ChannelUpdatePacket(string previousName, Channel channel) {
            PreviousName = previousName;
            Channel = channel;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Update);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PreviousName);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            yield return sb.ToString();
        }
    }
}
