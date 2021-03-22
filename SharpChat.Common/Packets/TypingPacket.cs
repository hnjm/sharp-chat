using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class TypingPacket : IServerPacket {
        public IChannel Channel { get; }
        public object TypingInfo { get; }

        public TypingPacket(IChannel channel, object typingInfo) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            TypingInfo = typingInfo ?? throw new ArgumentNullException(nameof(typingInfo));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.TypingInfo);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);
            sb.Append(IServerPacket.SEPARATOR);
            //sb.Append(TypingInfo.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            //sb.Append(TypingInfo.Started.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
