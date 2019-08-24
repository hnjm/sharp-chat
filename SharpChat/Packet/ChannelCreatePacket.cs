using System.Text;

namespace SharpChat.Packet
{
    public class ChannelCreatePacket : IServerPacket
    {
        public SockChatChannel Channel { get; private set; }

        public ChannelCreatePacket(SockChatChannel channel)
        {
            Channel = channel;
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerChannelPacket.Create);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channel.Pack(version));

            return sb.ToString();
        }
    }
}
