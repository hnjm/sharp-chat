using SharpChat.Channels;
using System.Text;

namespace SharpChat.Packets {
    public enum ContextClearMode {
        Messages = 0,
        Users = 1,
        Channels = 2,
        MessagesUsers = 3,
        MessagesUsersChannels = 4,
    }

    public class ContextClearPacket : IServerPacket {
        public IChannel Channel { get; private set; }
        public ContextClearMode Mode { get; private set; }

        public bool IsGlobal
            => Channel == null;

        public ContextClearPacket(IChannel channel, ContextClearMode mode) {
            Channel = channel;
            Mode = mode;
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextClear);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)Mode);
            sb.Append(IServerPacket.SEPARATOR);
            if(!IsGlobal)
                sb.Append(Channel.Name);

            return sb.ToString();
        }
    }
}
