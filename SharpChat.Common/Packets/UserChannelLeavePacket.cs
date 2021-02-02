using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserChannelLeavePacket : ServerPacketBase {
        public ChatUser User { get; private set; }

        public UserChannelLeavePacket(ChatUser user) {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.UserSwitch);
            sb.Append('\t');
            sb.Append((int)ServerMovePacket.UserLeft);
            sb.Append('\t');
            sb.Append(User.UserId);
            sb.Append('\t');
            sb.Append(SequenceId);

            yield return sb.ToString();
        }
    }
}
