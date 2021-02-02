using SharpChat.Users;

namespace SharpChat.Packets {
    public class BroadcastMessagePacket : BotResponsePacket {
        public BroadcastMessagePacket(IUser sender, string message)
            : base(sender, BotArguments.Notice(@"say", message)) { }
    }
}
