using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserChannelForceJoinPacket : ServerPacket {
        public ChatChannel Channel { get; private set; }

        public UserChannelForceJoinPacket(ChatChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)SockChatServerMovePacket.ForcedMove);
            sb.Append('\t');
            sb.Append(Channel.Name);

            yield return sb.ToString();
        }
    }
}
