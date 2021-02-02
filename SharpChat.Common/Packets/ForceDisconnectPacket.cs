﻿using System;
using System.Text;

namespace SharpChat.Packets {
    public enum ForceDisconnectReason {
        Kicked = 0,
        Banned = 1,
    }

    public class ForceDisconnectPacket : ServerPacketBase {
        public ForceDisconnectReason Reason { get; private set; }
        public DateTimeOffset Expires { get; private set; }

        public ForceDisconnectPacket(ForceDisconnectReason reason, DateTimeOffset? expires = null) {
            Reason = reason;

            if (reason == ForceDisconnectReason.Banned) {
                if (!expires.HasValue)
                    throw new ArgumentNullException(nameof(expires));
                Expires = expires.Value;
            }
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.BAKA);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)Reason);

            if (Reason == ForceDisconnectReason.Banned) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(Expires.ToUnixTimeSeconds());
            }

            return sb.ToString();
        }
    }
}
