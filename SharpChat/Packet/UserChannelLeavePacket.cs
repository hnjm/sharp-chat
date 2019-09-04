using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat.Packet
{
    public class UserChannelLeavePacket : IServerPacket
    {
        public SockChatUser User { get; private set; }

        public UserChannelLeavePacket(SockChatUser user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.UserSwitch);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerMovePacket.UserLeft);
            sb.Append(Constants.SEPARATOR);
            sb.Append(User.UserId);
            sb.Append(Constants.SEPARATOR);
            sb.Append(eventId);

            return new[] { sb.ToString() };
        }
    }
}
