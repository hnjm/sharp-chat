using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChannelCreatePacket : ServerPacket
    {
        public ChatChannel Channel { get; private set; }

        public ChannelCreatePacket(ChatChannel channel)
        {
            Channel = channel;
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append('\t');
            sb.Append((int)SockChatServerChannelPacket.Create);
            sb.Append('\t');
            sb.Append(Channel.Pack(version));

            return new[] { sb.ToString() };
        }
    }
}
