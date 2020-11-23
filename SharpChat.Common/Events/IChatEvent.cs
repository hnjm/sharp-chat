using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Flags]
    public enum ChatMessageFlags {
        None = 0,
        Action = 1,
        Broadcast = 1 << 1,
        Log = 1 << 2,
        Private = 1 << 3,
    }

    public interface IChatEvent {
        DateTimeOffset DateTime { get; set; }
        BasicUser Sender { get; set; }
        IPacketTarget Target { get; set; }
        string TargetName { get; set; }
        ChatMessageFlags Flags { get; set; }
        long SequenceId { get; set; }
    }

    public interface IChatMessage : IChatEvent {
        string Text { get; }
    }
}
