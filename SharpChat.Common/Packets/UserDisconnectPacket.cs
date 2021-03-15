using SharpChat.Users;
using System;
using System.Text;

namespace SharpChat.Packets {
    public enum UserDisconnectReason : int {
        Unknown = 0,
        Leave = 1,
        TimeOut = 2,
        Kicked = 3,
        Flood = 4,
    }

    public class UserDisconnectPacket : ServerPacketBase {
        public DateTimeOffset Disconnected { get; private set; }
        public ChatUser User { get; private set; }
        public UserDisconnectReason Reason { get; private set; }

        public UserDisconnectPacket(DateTimeOffset disconnected, ChatUser user, UserDisconnectReason reason) {
            Disconnected = disconnected;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Reason = reason;
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserDisconnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.DisplayName);
            sb.Append(IServerPacket.SEPARATOR);

            switch (Reason) {
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
            sb.Append(Disconnected.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);

            return sb.ToString();
        }
    }
}
