using System;
using System.Text;

namespace SharpChat.Packet
{
    public class UserUpdatePacket : IServerPacket
    {
        public SockChatUser User { get; private set; }

        public UserUpdatePacket(SockChatUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserUpdate);
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.Pack(version));

            return sb.ToString();
        }
    }
}
