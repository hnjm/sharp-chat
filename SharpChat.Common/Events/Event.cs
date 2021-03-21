using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public abstract string Type { get; }
        public DateTimeOffset DateTime { get; }
        public IUser User { get; }
        public IChannel Channel { get; }

        public Event(IChannel channel, IUser user, DateTimeOffset? dateTime = null) {
            EventId = SharpId.Next();
            DateTime = dateTime ?? DateTimeOffset.Now;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel; // channel is allowed to be NULL for broadcasting
        }
    }
}
