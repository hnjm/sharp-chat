using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet
{
    public class ContextChannelsPacket : IServerPacket
    {
        public IEnumerable<SockChatChannel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<SockChatChannel> channels)
        {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerContextPacket.Channels);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channels.Count());

            foreach (SockChatChannel channel in Channels)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(channel.Pack(version));
            }

            return sb.ToString();
        }
    }
}
