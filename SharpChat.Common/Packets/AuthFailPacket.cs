using SharpChat.Bans;
using System;
using System.Text;

namespace SharpChat.Packets {
    public enum AuthFailReason {
        AuthInvalid,
        MaxSessions,
        Banned,
    }

    public class AuthFailPacket : IServerPacket {
        public AuthFailReason Reason { get; private set; }
        public IBanRecord BanInfo { get; private set; }

        public AuthFailPacket(AuthFailReason reason, IBanRecord banInfo = null) {
            Reason = reason;

            if(reason == AuthFailReason.Banned)
                BanInfo = banInfo ?? throw new ArgumentNullException(nameof(banInfo));
        }

        public string Pack() {
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

                if (BanInfo.IsPermanent)
                    sb.Append(@"-1");
                else
                    sb.Append(BanInfo.Expires.ToUnixTimeSeconds());
            }

            return sb.ToString();
        }
    }
}
