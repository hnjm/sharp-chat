using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelDeleteEvent : Event {
        public const string TYPE = @"channel:delete";

        public override string Type => TYPE;

        public ChannelDeleteEvent(IChannel channel, IUser user) : base(channel, user) { }
    }
}
