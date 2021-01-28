using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Flags]
    public enum ChatEventFlags {
        None = 0,
        Action = 1,
        Broadcast = 1 << 1,
        Log = 1 << 2,
        Private = 1 << 3,
    }

    public interface IChatEvent {
        DateTimeOffset DateTime { get; set; }
        User Sender { get; set; }
        IPacketTarget Target { get; set; }
        string TargetName { get; set; }
        ChatEventFlags Flags { get; set; }
        long SequenceId { get; set; }
    }

    public interface IChatMessageEvent : IChatEvent {
        string Text { get; }
    }
}
