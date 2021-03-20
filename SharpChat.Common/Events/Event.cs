using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public abstract string Type { get; }
        public DateTimeOffset DateTime { get; }
        public IUser Sender { get; }
        public string Target { get; }

        public Event(IEventTarget target, IUser user, DateTimeOffset? dateTime = null) {
            EventId = SharpId.Next();
            DateTime = dateTime ?? DateTimeOffset.Now;
            Sender = user ?? throw new ArgumentNullException(nameof(user));
            Target = (target ?? throw new ArgumentNullException(nameof(target))).TargetName;
        }

        public virtual string EncodeAsJson()
            => @"{}";
    }
}
