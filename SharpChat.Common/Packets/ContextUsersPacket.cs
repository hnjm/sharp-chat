using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class ContextUsersPacket : IServerPacket {
        public IEnumerable<IUser> Users { get; private set; }

        public ContextUsersPacket(IEnumerable<IUser> users) {
            Users = users?.Where(u => u != null) ?? throw new ArgumentNullException(nameof(users));
        }

        public string Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append((int)ServerContextPacket.Users);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(Users.Count());

            foreach(IUser user in Users) {
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append(user.Pack());
                sb.Append(IServerPacket.SEPARATOR);
                sb.Append('1'); // visibility flag
            }

            return sb.ToString();
        }
    }
}
