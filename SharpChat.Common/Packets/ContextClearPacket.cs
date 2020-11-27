using SharpChat.Channels;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public enum ContextClearMode {
        Messages = 0,
        Users = 1,
        Channels = 2,
        MessagesUsers = 3,
        MessagesUsersChannels = 4,
    }

    public class ContextClearPacket : ServerPacket {
        public ChatChannel Channel { get; private set; }
        public ContextClearMode Mode { get; private set; }

        public bool IsGlobal
            => Channel == null;

        public ContextClearPacket(ChatChannel channel, ContextClearMode mode) {
            Channel = channel;
            Mode = mode;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextClear);
            sb.Append('\t');
            sb.Append((int)Mode);
            sb.Append('\t');
            sb.Append(Channel?.TargetName ?? string.Empty);

            yield return sb.ToString();
        }
    }
}
