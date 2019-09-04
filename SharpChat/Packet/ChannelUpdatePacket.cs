using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChannelUpdatePacket : IServerPacket
    {
        public string PreviousName { get; private set; }
        public SockChatChannel Channel { get; private set; }

        public ChannelUpdatePacket(string previousName, SockChatChannel channel)
        {
            PreviousName = previousName;
            Channel = channel;
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerChannelPacket.Update);
            sb.Append(Constants.SEPARATOR);
            sb.Append(PreviousName);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channel.Pack(version));

            return new[] { sb.ToString() };
        }
    }
}
