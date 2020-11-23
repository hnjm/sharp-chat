using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserUpdatePacket : ServerPacket {
        public ChatUser User { get; private set; }
        public string PreviousName { get; private set; }

        public UserUpdatePacket(ChatUser user, string previousName = null) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            PreviousName = previousName;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            bool isSilent = string.IsNullOrEmpty(PreviousName);

            if (!isSilent) {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append("\t-1\t0\fnick\f");
                sb.Append(PreviousName);
                sb.Append('\f');
                sb.Append(User.DisplayName);
                sb.Append('\t');
                sb.Append(SequenceId);
                sb.Append("\t10010");
                yield return sb.ToString();
                sb.Clear();
            }

            sb.Append((int)SockChatServerPacket.UserUpdate);
            sb.Append('\t');
            sb.Append(User.Pack());

            yield return sb.ToString();
        }
    }
}
