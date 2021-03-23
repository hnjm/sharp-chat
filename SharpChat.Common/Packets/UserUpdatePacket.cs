using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class UserUpdatePacket : ServerPacket {
        public IUser User { get; }

        public UserUpdatePacket(IUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.UserUpdate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());

            return sb.ToString();
        }
    }
}
