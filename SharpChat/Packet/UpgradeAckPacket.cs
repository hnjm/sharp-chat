using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class UpgradeAckPacket : ServerPacket {
        public bool Success { get; private set; }
        public int Version { get; private set; }

        public UpgradeAckPacket(bool success, int version) {
            Success = success;
            Version = version;
        }

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UpgradeAck);
            sb.Append('\t');
            sb.Append(Success ? '1' : '0');
            sb.Append('\t');
            sb.Append(Version);

            return new[] { sb.ToString() };
        }
    }
}
