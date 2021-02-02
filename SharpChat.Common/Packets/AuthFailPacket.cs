using System;
using System.Text;

namespace SharpChat.Packets {
    public enum AuthFailReason {
        AuthInvalid,
        MaxSessions,
        Banned,
    }

    public class AuthFailPacket : ServerPacketBase {
        public AuthFailReason Reason { get; private set; }
        public DateTimeOffset Expires { get; private set; }

        public AuthFailPacket(AuthFailReason reason, DateTimeOffset? expires = null) {
            Reason = reason;

            if (reason == AuthFailReason.Banned) {
                if (!expires.HasValue)
                    throw new ArgumentNullException(nameof(expires));
                Expires = expires.Value;
            }
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserConnect);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append('n');
            sb.Append(IServerPacket.SEPARATOR);

            switch (Reason) {
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

            if (Reason == AuthFailReason.Banned) {
                sb.Append(IServerPacket.SEPARATOR);

                if (Expires == DateTimeOffset.MaxValue)
                    sb.Append(@"-1");
                else
                    sb.Append(Expires.ToUnixTimeSeconds());
            }

            return sb.ToString();
        }
    }
}
