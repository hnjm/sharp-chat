using SharpChat.Users;
using System;

namespace SharpChat.Packets {
    public class BotResponsePacket : ChatMessageAddPacket {
        public BotResponsePacket(IUser sender, string stringId, bool isError = true, params object[] args)
            : this(sender, new BotArguments(isError, stringId, args)) { }

        public BotResponsePacket(IUser sender, BotArguments args)
            : base(sender, (args ?? throw new ArgumentNullException(nameof(args))).ToString()) { }
    }

    // Abbreviated class name because otherwise shit gets wide
    public static class LCR {
        public const string CHANNEL_INSUFFICIENT_HIERARCHY = @"ipchan";
        public const string CHANNEL_INVALID_PASSWORD = @"ipwchan";
    }
}
