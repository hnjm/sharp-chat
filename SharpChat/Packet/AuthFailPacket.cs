using System;
using System.Text;

namespace SharpChat.Packet
{
    public enum AuthFailReason
    {
        AuthInvalid,
        MaxSessions,
        Banned,
    }

    public class AuthFailPacket : IServerPacket
    {
        public AuthFailReason Reason { get; private set; }
        public DateTimeOffset Expires { get; private set; }

        public AuthFailPacket(AuthFailReason reason, DateTimeOffset? expires = null)
        {
            Reason = reason;

            if (reason == AuthFailReason.Banned)
            {
                if (!expires.HasValue)
                    throw new ArgumentNullException(nameof(expires));
                Expires = expires.Value;
            }
        }

        public string Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append(Constants.SEPARATOR);
            sb.Append('n');
            sb.Append(Constants.SEPARATOR);

            if (version >= 2) {
                switch (Reason)
                {
                    case AuthFailReason.AuthInvalid:
                    default:
                        sb.Append(@"auth");
                        break;
                    case AuthFailReason.MaxSessions:
                        sb.Append(@"conn");
                        break;
                    case AuthFailReason.Banned:
                        sb.Append(@"baka");
                        break;
                }
            } else
            {
                switch (Reason)
                {
                    case AuthFailReason.AuthInvalid:
                    default:
                        sb.Append(@"authfail");
                        break;
                    case AuthFailReason.MaxSessions:
                        sb.Append(@"sockfail");
                        break;
                    case AuthFailReason.Banned:
                        sb.Append(@"joinfail");
                        break;
                }
            }

            if(Reason == AuthFailReason.Banned)
            {
                sb.Append(Constants.SEPARATOR);

                if (Expires == DateTimeOffset.MaxValue)
                    sb.Append(@"-1");
                else
                    sb.Append(Expires.ToUnixTimeSeconds());
            }

            return sb.ToString();
        }
    }
}
