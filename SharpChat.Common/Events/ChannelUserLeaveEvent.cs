using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelUserLeaveEvent : Event {
        public const string TYPE = @"channel:user:leave";

        public UserDisconnectReason Reason { get; }

        public ChannelUserLeaveEvent(IChannel channel, IUser user, UserDisconnectReason reason)
            : base(
                  channel ?? throw new ArgumentNullException(nameof(channel)),
                  user ?? throw new ArgumentNullException(nameof(user))
              ) {
            Reason = reason;
        }
    }
}
