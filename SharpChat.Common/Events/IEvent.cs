using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public interface IEvent {
        DateTimeOffset DateTime { get; }
        IUser Sender { get; }
        IPacketTarget Target { get; }
        string TargetName { get; }
        EventFlags Flags { get; }
        long SequenceId { get; }
    }
}
