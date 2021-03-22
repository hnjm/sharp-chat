using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelForceJoinPacket : IServerPacket {
        public IChannel Channel { get; private set; }

        public ChannelForceJoinPacket(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerMovePacket.ForcedMove);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
