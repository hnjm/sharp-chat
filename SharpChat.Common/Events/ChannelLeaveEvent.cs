using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public class ChannelLeaveEvent : Event {
        public const string TYPE = @"channel:leave";

        public override string Type => TYPE;
        public UserDisconnectReason Reason { get; }

        public ChannelLeaveEvent(IChannel channel, IUser user, UserDisconnectReason reason)
            : base(channel ?? throw new ArgumentNullException(nameof(channel)), user) {
            Reason = reason;
        }
    }
}
