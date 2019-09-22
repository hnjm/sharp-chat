using System;

namespace SharpChat
{
    [Flags]
    public enum ChatMessageFlags
    {
        None = 0,
        Action = 1,
        Broadcast = 1 << 1,
        Log = 1 << 2,
        Private = 1 << 3,
    }

    public interface IChatMessage
    {
        string Text { get; }
        DateTimeOffset DateTime { get; }
        ChatUser Sender { get; }
        IPacketTarget Target { get;  }
        ChatMessageFlags Flags { get; }
        int SequenceId { get; set; }
    }
}
