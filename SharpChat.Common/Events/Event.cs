using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public DateTimeOffset DateTime { get; }
        public IUser User { get; }
        public IChannel Channel { get; }

        public Event(IChannel channel, IUser user, DateTimeOffset? dateTime = null) {
            EventId = SharpId.Next();
            DateTime = dateTime ?? DateTimeOffset.Now;
            User = user; // user is allowed to be NULL for things not involving users
            Channel = channel; // channel is allowed to be NULL for broadcasting
        }

        public override string ToString() {
            return $@"[{EventId}] {GetType().Name} {User} {Channel}";
        }
    }
}
