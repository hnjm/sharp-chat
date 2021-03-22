using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class UserConnectPacket : IServerPacket {
        private UserConnectEvent Connect { get; }

        public UserConnectPacket(UserConnectEvent connect) {
            Connect = connect ?? throw new ArgumentNullException(nameof(connect));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.User.Pack());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Connect.EventId);

            return sb.ToString();
        }
    }
}
