using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet {
    public class UserChannelLeavePacket : ServerPacket {
        public ChatUser User { get; private set; }

        public UserChannelLeavePacket(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)SockChatServerMovePacket.UserLeft);
            sb.Append('\t');
            sb.Append(User.UserId);
            sb.Append('\t');
            sb.Append(SequenceId);

            yield return sb.ToString();
        }
    }
}
