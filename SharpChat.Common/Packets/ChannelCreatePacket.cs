using SharpChat.Channels;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelCreatePacket : ServerPacketBase {
        public IChannel Channel { get; private set; }

        public ChannelCreatePacket(IChannel channel) {
            Channel = channel;
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Create);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            return sb.ToString();
        }
    }
}
