using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Packets {
    public class ChannelCreateResponsePacket : BotResponsePacket {
        public ChannelCreateResponsePacket(IUser sender, string channelName)
            : base(sender, BotArguments.Notice(@"crchan", channelName)) { }

        public ChannelCreateResponsePacket(IUser sender, Channel channel)
            : this(sender, channel.Name) { }
    }
}
