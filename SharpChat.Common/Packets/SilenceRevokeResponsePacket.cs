using SharpChat.Users;

namespace SharpChat.Packets {
    public class SilenceRevokeResponsePacket : BotResponsePacket {
        public SilenceRevokeResponsePacket(IUser sender, string userName)
            : base(sender, BotArguments.Notice(@"usilok", userName)) { }

        public SilenceRevokeResponsePacket(IUser sender, IUser target)
            : this(sender, target.DisplayName) { }
    }
}
