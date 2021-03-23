using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelDeleteEvent : Event {
        public const string TYPE = @"channel:delete";

        public ChannelDeleteEvent(IChannel channel, IUser user)
            : base(channel ?? throw new ArgumentNullException(nameof(channel)), user) { }
    }
}
