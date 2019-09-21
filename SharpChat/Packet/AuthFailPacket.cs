using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public enum AuthFailReason
    {
        AuthInvalid,
        MaxSessions,
        Banned,
    }

    public class AuthFailPacket : ServerPacket
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

        public override IEnumerable<string> Pack(int version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserConnect);
            sb.Append("\tn\t");

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
                sb.Append('\t');

                if (Expires == DateTimeOffset.MaxValue)
                    sb.Append(@"-1");
                else
                    sb.Append(Expires.ToSockChatSeconds(version));
            }

            return new[] { sb.ToString() };
        }
    }
}
