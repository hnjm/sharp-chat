using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class ChannelDeletePacket : IServerPacket
    {
        public SockChatChannel Channel { get; private set; }

        public ChannelDeletePacket(SockChatChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ChannelEvent);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerChannelPacket.Delete);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channel.Name);

            return new[] { sb.ToString() };
        }
    }
}
