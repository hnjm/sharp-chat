using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class ChannelJoinEvent : Event {
        public const string TYPE = @"channel:join";

        public override string Type => TYPE;

        public ChannelJoinEvent(IChannel channel, IUser user)
            : base(channel ?? throw new ArgumentNullException(nameof(channel)), user) {}
    }
}
