using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelLeaveEvent : Event {
        public const string TYPE = @"channel:leave";

        public UserDisconnectReason Reason { get; }

        public ChannelLeaveEvent(IChannel channel, IUser user, UserDisconnectReason reason)
            : base(
                  channel ?? throw new ArgumentNullException(nameof(channel)),
                  user ?? throw new ArgumentNullException(nameof(user))
              ) {
            Reason = reason;
        }
    }
}
