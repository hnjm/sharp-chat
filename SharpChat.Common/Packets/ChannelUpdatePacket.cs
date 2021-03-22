using SharpChat.Channels;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelUpdatePacket : IServerPacket {
        public string PreviousName { get; private set; }
        public IChannel Channel { get; private set; }

        public ChannelUpdatePacket(string previousName, IChannel channel) {
            PreviousName = previousName;
            Channel = channel;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Update);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PreviousName);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Pack());

            return sb.ToString();
        }
    }
}
