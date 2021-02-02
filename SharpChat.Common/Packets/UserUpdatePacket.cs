using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class UserUpdatePacket : ServerPacketBase {
        public ChatUser User { get; }

        public UserUpdatePacket(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserUpdate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());

            return sb.ToString();
        }
    }
}
