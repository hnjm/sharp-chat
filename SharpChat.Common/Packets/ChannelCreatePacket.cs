using SharpChat.Channels;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelCreatePacket : ServerPacketBase {
        public Channel Channel { get; private set; }

        public ChannelCreatePacket(Channel channel) {
            Channel = channel;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Create);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            yield return sb.ToString();
        }
    }
}
