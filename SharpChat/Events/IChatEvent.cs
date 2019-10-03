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
        DateTimeOffset DateTime { get; }
        ChatUser Sender { get; }
        IPacketTarget Target { get; }
        ChatMessageFlags Flags { get; }
        int SequenceId { get; set; }
    }

    public interface IChatMessage : IChatEvent {
        string Text { get; }
    }
}
