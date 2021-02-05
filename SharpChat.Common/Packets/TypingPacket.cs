using SharpChat.Channels;
using System;
using System.Text;

namespace SharpChat.Packets {
    public class TypingPacket : ServerPacketBase {
        public Channel Channel { get; }
        public ChannelTyping TypingInfo { get; }

        public TypingPacket(Channel channel, ChannelTyping typingInfo) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            TypingInfo = typingInfo ?? throw new ArgumentNullException(nameof(typingInfo));
        }

        public override string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.TypingInfo);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Channel.Name);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(TypingInfo.User.UserId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(TypingInfo.Started.ToUnixTimeSeconds());

            return sb.ToString();
        }
    }
}
