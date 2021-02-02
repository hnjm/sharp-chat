using SharpChat.Bans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChat.Packets {
    public class BanListPacket : ServerPacketBase {
        public IEnumerable<IBan> Bans { get; private set; }

        public BanListPacket(IEnumerable<IBan> bans) {
            Bans = bans ?? throw new ArgumentNullException(nameof(bans));
        }

        public override IEnumerable<string> Pack() {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)ServerPacket.MessageAdd);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(DateTimeOffset.Now.ToUnixTimeSeconds());
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(-1); // HERE
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(BotArguments.Notice(@"banlist", string.Join(", ", Bans.Select(
                b => string.Format(@"<a href=""javascript:void(0);"" onclick=""Chat.SendMessageWrapper('/unban '+ this.innerHTML);"">{0}</a>, ", b)
            ))));
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(SequenceId);
            sb.Append(IServerPacket.SEPARATOR);
            sb.Append(@"10010");

            return new[] { sb.ToString() };
        }
    }
}
