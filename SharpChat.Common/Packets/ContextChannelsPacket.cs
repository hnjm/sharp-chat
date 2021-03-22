using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class ContextChannelsPacket : IServerPacket {
        public IEnumerable<IChannel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<IChannel> channels) {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextPacket.Channels);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channels.Count());

            foreach(IChannel channel in Channels) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(channel.Pack());
            }

            return sb.ToString();
        }
    }
}
