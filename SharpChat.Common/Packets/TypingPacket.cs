using SharpChat.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class TypingPacket : ServerPacket {
        public Channel Channel { get; }
        public ChannelTyping TypingInfo { get; }

        public TypingPacket(Channel channel, ChannelTyping typingInfo) {
            Channel = channel;
            TypingInfo = typingInfo ?? throw new ArgumentNullException(nameof(typingInfo));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.Typing);
            sb.Append('\t');
            sb.Append(Channel?.TargetName ?? string.Empty);
            sb.Append('\t');
            sb.Append(TypingInfo.User.UserId);
            sb.Append('\t');
            sb.Append(TypingInfo.Started.ToUnixTimeSeconds());

            yield return sb.ToString();
        }
    }
}
