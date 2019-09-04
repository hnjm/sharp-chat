using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public enum ForceDisconnectReason
    {
        Kicked = 0,
        Banned = 1,
    }

    public class ForceDisconnectPacket : IServerPacket
    {
        public ForceDisconnectReason Reason { get; private set; }
        public DateTimeOffset Expires { get; private set; }

        public ForceDisconnectPacket(ForceDisconnectReason reason, DateTimeOffset? expires = null)
        {
            Reason = reason;

            if (reason == ForceDisconnectReason.Banned)
            {
                if (!expires.HasValue)
                    throw new ArgumentNullException(nameof(expires));
                Expires = expires.Value;
            }
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.BAKA);
            sb.Append(Constants.SEPARATOR);

            if (version >= 2)
                sb.Append((int)Reason);
            else
                sb.Append(Reason == ForceDisconnectReason.Banned ? '1' : '0');

            if(Reason == ForceDisconnectReason.Banned)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(Expires.ToUnixTimeSeconds());
            }

            return new[] { sb.ToString() };
        }
    }
}
