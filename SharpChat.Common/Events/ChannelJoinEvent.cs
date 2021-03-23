using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelJoinEvent : Event {
        public const string TYPE = @"channel:join";

        public ChannelJoinEvent(IChannel channel, IUser user)
            : base(
                  channel ?? throw new ArgumentNullException(nameof(channel)),
                  user ?? throw new ArgumentNullException(nameof(user))
             ) {}
    }
}
