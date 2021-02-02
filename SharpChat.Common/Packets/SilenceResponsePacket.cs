using SharpChat.Users;

namespace SharpChat.Packets {
    public class SilenceResponsePacket : BotResponsePacket {
        public SilenceResponsePacket(IUser sender, string userName)
            : base(sender, BotArguments.Notice(@"silok", userName)) { }

        public SilenceResponsePacket(IUser sender, IUser target)
            : this(sender, target.DisplayName) { }
    }
}
