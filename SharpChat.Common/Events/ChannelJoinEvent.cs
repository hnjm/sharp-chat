using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelJoinEvent : Event, IChannelEvent {
        public const string TYPE = @"channel:join";

        public override string Type => TYPE;

        private ChannelJoinEvent(IEvent evt) : base(evt) {}
        public ChannelJoinEvent(Channel channel, IUser user) : base(channel, user) {}

        public static ChannelJoinEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new ChannelJoinEvent(evt);
        }
    }
}
