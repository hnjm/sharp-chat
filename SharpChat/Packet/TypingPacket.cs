using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class TypingPacket : ServerPacket {
        public TypingPacket() {
            //
        }

        public override IEnumerable<string> Pack(int version) {
            if (version < 2)
                return null;

            StringBuilder sb = new StringBuilder();

            return new[] { sb.ToString() };
        }
    }
}
