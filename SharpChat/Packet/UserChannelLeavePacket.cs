using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserChannelLeavePacket : ServerPacket
    {
        public SockChatUser User { get; private set; }

        public UserChannelLeavePacket(SockChatUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)SockChatServerMovePacket.UserLeft);
            sb.Append('\t');
            sb.Append(User.UserId);
            sb.Append('\t');
            sb.Append(SequenceId);

            return new[] { sb.ToString() };
        }
    }
}
