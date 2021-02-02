using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class ContextUsersPacket : ServerPacketBase {
        public IEnumerable<ChatUser> Users { get; private set; }

        public ContextUsersPacket(IEnumerable<ChatUser> users) {
            Users = users?.Where(u => u != null) ?? throw new ArgumentNullException(nameof(users));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.ContextPopulate);
            sb.Append('\t');
            sb.Append((int)ServerContextPacket.Users);
            sb.Append('\t');
            sb.Append(Users.Count());

            foreach (ChatUser user in Users) {
                sb.Append('\t');
                sb.Append(user.Pack());
                sb.Append('\t');
                sb.Append('1'); // visibility flag
            }

            yield return sb.ToString();
        }
    }
}
