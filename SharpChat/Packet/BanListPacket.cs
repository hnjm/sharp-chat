using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet {
    public class BanListPacket : ServerPacket {
        public IEnumerable<IBan> Bans { get; private set; }
        public IEnumerable<ChatUser> Users { get; private set; }

        public BanListPacket(IEnumerable<IBan> bans, IEnumerable<ChatUser> users) {
            Bans = bans ?? throw new ArgumentNullException(nameof(bans));
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public override IEnumerable<string> Pack(int version) {
            StringBuilder sb = new StringBuilder();

            if (version >= 2) {
                // construct proper packet
            } else {
                sb.Append((int)SockChatServerPacket.MessageAdd);
                sb.Append('\t');
                sb.Append(DateTimeOffset.Now.ToSockChatSeconds(version));
                sb.Append("\t-1\t0\fbanlist\f");

                foreach (IBan ban in Bans) {
                    string text = string.Empty;

                    if (ban is BannedUser banUser)
                        text = Users.FirstOrDefault(x => x.UserId == banUser.UserId)?.Username ?? string.Format(@"@{0}", banUser.UserId);
                    else if (ban is BannedIPAddress banIp)
                        text = banIp.Address.ToString();

                    sb.AppendFormat(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", text);
                }

                if (Bans.Any())
                    sb.Length -= 2;

                sb.Append('\t');
                sb.Append(SequenceId);
                sb.Append("\t10010");
            }

            return new[] { sb.ToString() };
        }
    }
}
