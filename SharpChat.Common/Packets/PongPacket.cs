﻿using SharpChat.Events;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class PongPacket : ServerPacket {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(SessionPingEvent spe) {
            PongTime = spe.DateTime;
        }

        protected override string DoPack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacketId.Pong);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PongTime.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
