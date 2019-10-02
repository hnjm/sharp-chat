using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class UserChannelJoinPacket : ServerPacket {
        public ChatUser User { get; private set; }

        public UserChannelJoinPacket(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)SockChatServerMovePacket.UserJoined);
            sb.Append('\t');
            sb.Append(User.UserId);

            if (version < 2) {
                sb.Append('\t');
                sb.Append(User.GetDisplayName(version));
                sb.Append('\t');
                sb.Append(User.Colour);
            }

            sb.Append('\t');
            sb.Append(SequenceId);

            return new[] { sb.ToString() };
        }
    }
}
