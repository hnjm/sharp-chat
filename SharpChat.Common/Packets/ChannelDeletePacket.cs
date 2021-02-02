using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class ChannelDeletePacket : ServerPacketBase {
        public Channel Channel { get; private set; }

        public ChannelDeletePacket(Channel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ChannelEvent);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerChannelPacket.Delete);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);

            yield return sb.ToString();
        }
    }
}
