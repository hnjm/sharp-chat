using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public enum ContextClearMode {
        Messages = 0,
        Users = 1,
        Channels = 2,
        MessagesUsers = 3,
        MessagesUsersChannels = 4,
    }

    public class ContextClearPacket : ServerPacket {
        public ContextClearMode Mode { get; private set; }

        public ContextClearPacket(ContextClearMode mode) {
            Mode = mode;
        }

        public override IEnumerable<string> Pack(int version) {
            if (version > 1)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextClear);
            sb.Append('\t');
            sb.Append((int)Mode);

            return new[] { sb.ToString() };
        }
    }
}
