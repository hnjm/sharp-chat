using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packets {
    public class UserUpdatePacket : ServerPacketBase {
        public ChatUser User { get; private set; }
        public string PreviousName { get; private set; }

        public UserUpdatePacket(ChatUser user, string previousName = null) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            PreviousName = previousName;
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            if(!string.IsNullOrEmpty(PreviousName)) {
                sb.Append((int)ServerPacket.MessageAdd);
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(-1); // HERE
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(BotArguments.Notice(@"nick", PreviousName, User.DisplayName));
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(SequenceId);
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(@"10010");
                yield return sb.ToString();
                sb.Clear();
            }

            sb.Append((int)ServerPacket.UserUpdate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(User.Pack());

            yield return sb.ToString();
        }
    }
}
