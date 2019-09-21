using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserChannelForceJoinPacket : ServerPacket
    {
        public SockChatChannel Channel { get; private set; }

        public UserChannelForceJoinPacket(SockChatChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)SockChatServerMovePacket.ForcedMove);
            sb.Append('\t');
            sb.Append(Channel.Name);

            return new[] { sb.ToString() };
        }
    }
}
