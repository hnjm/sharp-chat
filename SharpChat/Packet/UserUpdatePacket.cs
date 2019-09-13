using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserUpdatePacket : IServerPacket
    {
        public SockChatUser User { get; private set; }
        public string PreviousName { get; private set; }

        public UserUpdatePacket(SockChatUser user, string previousName = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            PreviousName = previousName;
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            string[] lines = new string[2];

            StringBuilder sb = new StringBuilder();

            bool isSilent = string.IsNullOrEmpty(PreviousName);

            if (version < 2 && !isSilent)
            {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append(Constants.SEPARATOR);
                sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
                sb.Append(Constants.SEPARATOR);
                sb.Append(-1);
                sb.Append(Constants.SEPARATOR);
                sb.Append("0\fnick\f");
                sb.Append(PreviousName);
                sb.Append('\f');
                sb.Append(User.GetDisplayName(version));
                sb.Append(Constants.SEPARATOR);
                sb.Append(eventId);
                sb.Append(Constants.SEPARATOR);
                sb.Append(SockChatMessageFlags.RegularUser.Serialise());
                lines[0] = sb.ToString();
                sb.Clear();
            }

            sb.Append((int)SockChatServerPacket.UserUpdate);
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.Pack(version));

            if(version >= 2)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(isSilent ? '1' : '0');
            }

            lines[1] = sb.ToString();

            return lines;
        }
    }
}
