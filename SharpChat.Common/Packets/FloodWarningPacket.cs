using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class FloodWarningPacket : ServerPacketBase {
        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(-1); // HERE
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(BotArguments.Notice(@"flwarn"));
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(@"10010");

            yield return sb.ToString();
        }
    }
}
