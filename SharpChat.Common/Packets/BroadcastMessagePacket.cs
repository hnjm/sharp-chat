using SharpChat.Events;

namespace SharpChat.Packets {
    public class BroadcastMessagePacket : BotResponsePacket {
        public BroadcastMessagePacket(BroadcastMessageEvent broadcast)
            : base(broadcast.User, BotArguments.Notice(@"say", broadcast.Text)) { }
    }
}
