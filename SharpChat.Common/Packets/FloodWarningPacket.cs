using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class FloodWarningPacket : ServerPacketBase {
        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append('\t');
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append('\t');
            sb.Append(-1);
            sb.Append("\t0\fflwarn\t");
            sb.Append(SequenceId);
            sb.Append("\t10010");

            yield return sb.ToString();
        }
    }
}
