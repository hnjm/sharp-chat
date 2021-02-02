using SharpChat.Users;

namespace SharpChat.Packets {
    public class ChannelRankResponsePacket : BotResponsePacket {
        public ChannelRankResponsePacket(IUser sender)
            : base(sender, BotArguments.Notice(@"cprivchan")) {}
    }
}
