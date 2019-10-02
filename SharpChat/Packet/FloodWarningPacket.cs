using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class FloodWarningPacket : ServerPacket {
        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            if (version >= 2)
                sb.Append((int)SockChatServerPacket.FloodWarning);
            else {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToSockChatSeconds(version));
                sb.Append("\t-1\t0\fflwarn\t");
                sb.Append(SequenceId);
                sb.Append("\t10010");
            }

            return new[] { sb.ToString() };
        }
    }
}
