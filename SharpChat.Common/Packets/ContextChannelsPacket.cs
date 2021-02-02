using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class ContextChannelsPacket : ServerPacketBase {
        public IEnumerable<Channel> Channels { get; private set; }

        public ContextChannelsPacket(IEnumerable<Channel> channels) {
            Channels = channels?.Where(c => c != null) ?? throw new ArgumentNullException(nameof(channels));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextPacket.Channels);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channels.Count());

            foreach (Channel channel in Channels) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(channel.Pack());
            }

            yield return sb.ToString();
        }
    }
}
