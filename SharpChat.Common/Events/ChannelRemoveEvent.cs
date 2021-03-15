using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelRemoveEvent : Event, IChannelEvent {
        public const string TYPE = @"channel:remove";

        public override string Type => TYPE;

        private ChannelRemoveEvent(IEvent evt) : base(evt) { }
        public ChannelRemoveEvent(Channel channel, IUser user) : base(channel, user) { }

        public static ChannelRemoveEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new ChannelRemoveEvent(evt);
        }
    }
}
