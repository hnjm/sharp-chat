using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserConnectPacket : ServerPacket
    {
        public DateTimeOffset Joined { get; private set; }
        public SockChatUser User { get; private set; }

        public UserConnectPacket(DateTimeOffset joined, SockChatUser user)
        {
            Joined = joined;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append('\t');
            sb.Append(Joined.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(User.Pack(version));
            sb.Append('\t');
            sb.Append(SequenceId);

            return new[] { sb.ToString() };
        }
    }
}
