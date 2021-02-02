using SharpChat.Users;

namespace SharpChat.Packets {
    public class SilenceNoticePacket : BotResponsePacket {
        public SilenceNoticePacket(IUser sender)
            : base(sender, BotArguments.Notice(@"silence")) {}
    }
}
