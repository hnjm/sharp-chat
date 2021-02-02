using SharpChat.Users;

namespace SharpChat.Packets {
    public class ChannelPasswordResponsePacket : BotResponsePacket {
        public ChannelPasswordResponsePacket(IUser sender)
            : base(sender, BotArguments.Notice(@"cpwdchan")) { }
    }
}
