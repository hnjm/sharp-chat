using System;
using System.Text;

namespace SharpChat.Packets {
    public class PongPacket : IServerPacket {
        public DateTimeOffset PongTime { get; private set; }

        public PongPacket(DateTimeOffset dto) {
            PongTime = dto;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.Pong);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(PongTime.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
