using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelLeaveEvent : Event, IChannelEvent {
        public const string TYPE = @"channel:leave";

        public override string Type => TYPE;

        private ChannelLeaveEvent(IEvent evt) : base(evt) { }
        public ChannelLeaveEvent(Channel channel, IUser user) : base(channel, user) {}

        public static ChannelLeaveEvent DecodeFromJson(IEvent evt, JsonElement elem) {
            return new ChannelLeaveEvent(evt);
        }
    }
}
