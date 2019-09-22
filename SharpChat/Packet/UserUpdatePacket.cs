using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserUpdatePacket : ServerPacket
    {
        public ChatUser User { get; private set; }
        public string PreviousName { get; private set; }

        public UserUpdatePacket(ChatUser user, string previousName = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            PreviousName = previousName;
        }

        public override IEnumerable<string> Pack(int version)
        {
            string[] lines = new string[2];

            StringBuilder sb = new StringBuilder();

            bool isSilent = string.IsNullOrEmpty(PreviousName);

            if (version < 2 && !isSilent)
            {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToSockChatSeconds(version));
                sb.Append("\t-1\t0\fnick\f");
                sb.Append(PreviousName);
                sb.Append('\f');
                sb.Append(User.GetDisplayName(version));
                sb.Append('\t');
                sb.Append(SequenceId);
                sb.Append("\t10010");
                lines[0] = sb.ToString();
                sb.Clear();
            }

            sb.Append((int)SockChatServerPacket.UserUpdate);
            sb.Append('\t');
            sb.Append(User.Pack(version));

            if(version >= 2)
            {
                sb.Append('\t');
                sb.Append(isSilent ? '1' : '0');
            }

            lines[1] = sb.ToString();

            return lines;
        }
    }
}
