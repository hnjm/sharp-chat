using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public interface IEvent {
        DateTimeOffset DateTime { get; }
        IUser Sender { get; }
        Channel Target { get; }
        string TargetName { get; }
        EventFlags Flags { get; }
        long EventId { get; }
    }
}
