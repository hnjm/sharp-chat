using SharpChat.Users;

namespace SharpChat.Packets {
    public class FloodWarningPacket : BotResponsePacket {
        public FloodWarningPacket(IUser sender)
            : base(sender, BotArguments.Notice(@"flwarn")) { }
    }
}
