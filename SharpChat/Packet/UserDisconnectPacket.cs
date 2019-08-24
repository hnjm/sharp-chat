using System;
using System.Text;

namespace SharpChat.Packet
{
    public enum UserDisconnectReason
    {
        Leave,
        TimeOut,
        Kicked,
        Flood,
    }

    public class UserDisconnectPacket : IServerPacket
    {
        public DateTimeOffset Disconnected { get; private set; }
        public SockChatUser User { get; private set; }
        public UserDisconnectReason Reason { get; private set; }

        public UserDisconnectPacket(DateTimeOffset disconnected, SockChatUser user, UserDisconnectReason reason)
        {
            Disconnected = disconnected;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Reason = reason;
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserDisconnect);
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(Constants.SEPARATOR);

            if (version < 2)
            {
                sb.Append(User.DisplayName);
                sb.Append(Constants.SEPARATOR);
            }

            switch(Reason)
            {
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

            sb.Append(Constants.SEPARATOR);
            sb.Append(Disconnected.ToUnixTimeSeconds());
            sb.Append(Constants.SEPARATOR);
            sb.Append(eventId);

            return sb.ToString();
        }
    }
}
