using SharpChat.Users;
using System;

namespace SharpChat.Packets {
    public class BotResponsePacket : ChatMessageAddPacket {
        public BotResponsePacket(IUser sender, string stringId, bool isError = true, params object[] args)
            : this(sender, new BotArguments(isError, stringId, args)) { }

        public BotResponsePacket(IUser sender, BotArguments args)
            : base(sender, (args ?? throw new ArgumentNullException(nameof(args))).ToString()) { }
    }
}
