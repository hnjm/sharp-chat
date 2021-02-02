using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class PongPacket : ServerPacketBase {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(DateTimeOffset dto) {
            PongTime = dto;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.Pong);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PongTime.ToUnixTimeSeconds());

            yield return sb.ToString();
        }
    }
}
