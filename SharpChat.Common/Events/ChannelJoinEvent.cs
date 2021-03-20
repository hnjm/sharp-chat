using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelJoinEvent : Event {
        public const string TYPE = @"channel:join";

        public override string Type => TYPE;

        public ChannelJoinEvent(IChannel channel, IUser user) : base(channel, user) {}
    }
}
