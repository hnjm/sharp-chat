using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelDeletePacket : IServerPacket {
        public IChannel Channel { get; private set; }

        public ChannelDeletePacket(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Delete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
