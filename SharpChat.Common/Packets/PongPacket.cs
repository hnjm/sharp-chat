using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class PongPacket : ServerPacket {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(DateTimeOffset dto) {
            PongTime = dto;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.Pong);
            sb.Append('\t');
            sb.Append(PongTime.ToUnixTimeSeconds());

            yield return sb.ToString();
        }
    }
}
