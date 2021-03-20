using SharpChat.Channels;
using SharpChat.Users;
using System.Text.Json;

namespace SharpChat.Events {
    public class ChannelLeaveEvent : Event {
        public const string TYPE = @"channel:leave";

        public override string Type => TYPE;

        public ChannelLeaveEvent(IChannel channel, IUser user) : base(channel, user) {}
    }
}
