using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packet {
    public class BanListPacket : ServerPacket {
        public IEnumerable<IBan> Bans { get; private set; }

        public BanListPacket(IEnumerable<IBan> bans) {
            Bans = bans ?? throw new ArgumentNullException(nameof(bans));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)SockChatServerPacket.MessageAdd);
            sb.Append('\t');
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append("\t-1\t0\fbanlist\f");

            foreach (IBan ban in Bans)
                sb.AppendFormat(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", ban);

            if (Bans.Any())
                sb.Length -= 2;

            sb.Append('\t');
            sb.Append(SequenceId);
            sb.Append("\t10010");

            return new[] { sb.ToString() };
        }
    }
}
