using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet
{
    public class ContextUsersPacket : IServerPacket
    {
        public IEnumerable<SockChatUser> Users { get; private set; }

        public ContextUsersPacket(IEnumerable<SockChatUser> users)
        {
            Users = users?.Where(u => u != null) ?? throw new ArgumentNullException(nameof(users));
        }

        public IEnumerable<string> Pack(int version, int eventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.ContextPopulate);
            sb.Append(Constants.SEPARATOR);
            sb.Append((int)SockChatServerContextPacket.Users);
            sb.Append(Constants.SEPARATOR);
            sb.Append(Users.Count());

            foreach(SockChatUser user in Users)
            {
                sb.Append(Constants.SEPARATOR);
                sb.Append(user.Pack(version));
                sb.Append(Constants.SEPARATOR);
                sb.Append('1'); // visibility flag
            }

            return new[] { sb.ToString() };
        }
    }
}
