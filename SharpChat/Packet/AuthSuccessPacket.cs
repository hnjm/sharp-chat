using System;
using System.Text;

namespace SharpChat.Packet
{
    public class AuthSuccessPacket : IServerPacket
    {
        public SockChatUser User { get; private set; }
        public SockChatChannel Channel { get; private set; }

        public AuthSuccessPacket(SockChatUser user, SockChatChannel channel)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append(Constants.SEPARATOR);
            sb.Append('y');
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.Pack(version));
            sb.Append(Constants.SEPARATOR);
            sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
