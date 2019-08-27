using System;

namespace SharpChat
{
    public interface IChatMessage
    {
        int MessageId { get; }
        string Text { get; }
        DateTimeOffset DateTime { get; }
        SockChatMessageFlags Flags { get; }
        SockChatChannel Channel { get;  }
        SockChatUser User { get; }
    }
}
