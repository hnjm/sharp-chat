using SharpChat.Bans;
using SharpChat.Users;

namespace SharpChat.Packets {
    public class PardonResponsePacket : BotResponsePacket {
        public PardonResponsePacket(IUser sender, IBan ban)
            : base(sender, BotArguments.Notice(@"unban", ban)) { }
    }
}
