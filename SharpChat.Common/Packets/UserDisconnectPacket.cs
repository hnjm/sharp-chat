using SharpChat.Events;
using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class UserDisconnectPacket : IServerPacket {
        private UserDisconnectEvent Disconnect { get; }

        public UserDisconnectPacket(UserDisconnectEvent disconnect) {
            Disconnect = disconnect ?? throw new ArgumentNullException(nameof(disconnect));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserDisconnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.User.GetDisplayName());
            sb.Append(IServerPacket.SEPARATOR);

            switch(Disconnect.Reason) {
                case UserDisconnectReason.Leave:
                default:
                    sb.Append(@"leave");
                    break;
                case UserDisconnectReason.TimeOut:
                    sb.Append(@"timeout");
                    break;
                case UserDisconnectReason.Kicked:
                    sb.Append(@"kick");
                    break;
                case UserDisconnectReason.Flood:
                    sb.Append(@"flood");
                    break;
            }

            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.DateTime.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Disconnect.EventId);

            return sb.ToString();
        }
    }
}
