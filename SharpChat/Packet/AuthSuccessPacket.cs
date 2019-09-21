using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class AuthSuccessPacket : ServerPacket
    {
        public SockChatUser User { get; private set; }
        public SockChatChannel Channel { get; private set; }

        public AuthSuccessPacket(SockChatUser user, SockChatChannel channel)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append("\ty\t");
            sb.Append(User.Pack(version));
            sb.Append('\t');
            sb.Append(Channel.Name);

            return new[] { sb.ToString() };
        }
    }
}
