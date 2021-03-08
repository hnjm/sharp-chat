using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpChat.Packets {
    public class SwitchServerPacket : ServerPacketBase {
        public SwitchServerPacket() {
            // definition unfinished
            // optional argument containing server, if no argument assume client has a list?
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.SwitchServer);

            return sb.ToString();
        }
    }
}
